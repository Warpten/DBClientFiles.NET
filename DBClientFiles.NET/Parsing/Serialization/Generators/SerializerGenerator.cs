using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions.Extensions;

namespace DBClientFiles.NET.Parsing.Serialization.Generators
{
    internal abstract class TypedSerializerGenerator<T, TMethod> : SerializerGenerator where TMethod : Delegate
    {
        public TypedSerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
            Debug.Assert(typeof(T) == root.Type);
        }

        protected override void PrepareMethodParameters()
        {
            var methodType = typeof(TMethod).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            if (methodType != null)
            {
                var methodParams = methodType.GetParameters();
                foreach (var methodParam in methodParams)
                    Parameters.Add(Expression.Parameter(methodParam.ParameterType, methodParam.Name));
            }
        }

        public TMethod GenerateDeserializer()
        {
            var body = GenerateDeserializationMethodBody();
            
#if DEBUG
            // Meh
            var header = string.Join(", ", Parameters.Select(p => string.Join(' ', p.Type.Name.Replace("`1", $"<{Instance.Type.Name}>"), p.Name)));
            Console.WriteLine($"({header}) => ");
            Console.Write(body.AsString());
#endif

            return Expression.Lambda<TMethod>(body, Parameters).Compile();
        }

        protected override TreeNode MakeRootNode()
        {
            return new TreeNode() {
                AccessExpression = Instance,
                MemberToken = null,
                Parent = null,
                TypeToken = Root
            };
        }

        protected override sealed Expression MakeRootMemberAccess(MemberToken token)
        {
            return Expression.MakeMemberAccess(Instance, token.MemberInfo);
        }

        protected override sealed Expression MakeReturnExpression()
        {
            return Instance;
        }

        protected Expression RecordReader => Parameters[0];
        protected Expression FileParser => Parameters[1];

        private Expression Instance => Parameters[2];
    }

    /// <summary>
    /// Generates serialization methods.
    /// </summary>
    internal abstract class SerializerGenerator
    {
        protected TypeToken Root { get; }
        private TypeTokenType MemberType { get; }

        public SerializerGenerator(TypeToken root, TypeTokenType memberType)
        {
            Root = root;
            MemberType = memberType;

            PrepareMethodParameters();
        }

        /// <summary>
        /// Prepares the set of <see cref="ParameterExpression"/> for the method's prototype.
        /// </summary>
        protected abstract void PrepareMethodParameters();

        /// <summary>
        /// Generates the method's body
        /// </summary>
        /// <returns></returns>
        protected Expression GenerateDeserializationMethodBody()
        {
            // Generate a tree of BodyExpression.
            // We just populate the type token, the member info, and the access expressions
            var bodyParts = MakeRootNode();

            foreach (var member in Root.Members)
            {
                if (member.MemberType != MemberType || member.IsReadOnly)
                    continue;

                var node = new TreeNode() {
                    AccessExpression = MakeRootMemberAccess(member),
                    TypeToken = member.TypeToken,
                    MemberToken = member,
                };

                GenerateTreeNode(node);
                bodyParts.Children.Add(node);
            }

            // Emit candidate read calls.
            bodyParts.GenerateReadCalls(this);
            //GenerateReadCalls(bodyParts.Children); // Reference implementation of above

            TryUnrollLoops(bodyParts);

            var expr = bodyParts.ToExpression();
            var returnType = MakeReturnExpression();
            if (returnType != null)
                expr = Expression.Block(expr, returnType);

            // TODO: See if trying to optimize iterator usages in the generated code outweights the cost of optimizing
            // (It probably doesn't)
            return expr;
        }

        /// <summary>
        /// Performs loop unrolling as needed in the tree.
        /// </summary>
        /// <param name="nodes"></param>
        private void TryUnrollLoops(TreeNode node)
        {
            for (var i = 0; i < node.Children.Count; ++i)
            {
                // Try to unroll the loop if it is necessary
                node.Children[i] = node.Children[i].TryUnroll();

                // Update parent ref
                node.Children[i].Parent = node;
                // And finally try to unroll the child's children.
                TryUnrollLoops(node.Children[i]);
            }
        }

        /// <summary>
        /// Generates read calls for each of the properties declared in the tree.
        /// </summary>
        /// <param name="nodes"></param>
        private void GenerateReadCalls(List<TreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.TypeToken != null)
                {
                    var reader = GenerateExpressionReader(node.TypeToken, node.MemberToken);
                    if (reader != null)
                        node.ReadExpression.Add(reader);
                }

                if (node.Children.Count > 0)
                {
                    var invocationCount = 1;
                    if (node is LoopTreeNode loopNode)
                        invocationCount = loopNode.IterationCount - loopNode.InitialValue;

                    for (var i = 0; i < invocationCount; ++i)
                        GenerateReadCalls(node.Children);
                }
            }
        }

        /// <summary>
        /// Generates a node in the tree representation of the structure.
        /// </summary>
        /// <param name="parent"></param>
        private void GenerateTreeNode(TreeNode parent)
        {
            if (parent.TypeToken.IsArray)
            {
                var loopNode = new LoopTreeNode() {
                    Array = parent,
                    InitialValue = 0,
                    IterationCount = parent.MemberToken.Cardinality,
                    Parent = parent,
                    MemberToken = parent.MemberToken,
                    TypeToken = parent.TypeToken.GetElementTypeToken()
                };

                var elementTypeToken = parent.TypeToken.GetElementTypeToken();
                foreach (var member in elementTypeToken.Members)
                {
                    if (member.MemberType != MemberType || member.IsReadOnly)
                        continue;

                    var node = new TreeNode() {
                        AccessExpression = Expression.MakeMemberAccess(loopNode.AccessExpression, member.MemberInfo),
                        MemberToken = member,
                        TypeToken = member.TypeToken,
                        Parent = loopNode
                    };

                    GenerateTreeNode(node);

                    loopNode.Children.Add(node);
                }

                parent.Children.Add(loopNode);
            }
            else
            {
                foreach (var member in parent.TypeToken.Members)
                {
                    if (member.MemberType != MemberType || member.IsReadOnly)
                        continue;

                    var node = new TreeNode() {
                        AccessExpression = Expression.MakeMemberAccess(parent.AccessExpression, member.MemberInfo),
                        MemberToken = member,
                        TypeToken = member.TypeToken,
                        Parent = parent
                    };

                    GenerateTreeNode(node);
                    parent.Children.Add(node);
                }
            }
        }

        protected abstract TreeNode MakeRootNode();

        /// <summary>
        /// Returns the final expression of the method's body.
        /// </summary>
        /// <returns></returns>
        protected abstract Expression MakeReturnExpression();

        /// <summary>
        /// Provides access to the given member token on the root structure that is being deserialized.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Expression MakeRootMemberAccess(MemberToken token);

        /// <summary>
        /// Generates a deserialization call expression for the provided element of the tree.
        /// </summary>
        /// <param name="memberToken"></param>
        /// <returns></returns>
        public abstract Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken);

        protected List<ParameterExpression> Parameters { get; } = new List<ParameterExpression>();
    }
}
