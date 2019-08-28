using System.Collections.Generic;
using System.Linq.Expressions;
using DBClientFiles.NET.Parsing.Reflection;

namespace DBClientFiles.NET.Parsing.Serialization.Generators
{

    /// <summary>
    /// Generates serialization methods for various objects.
    /// </summary>
    internal abstract class SerializerGenerator
    {
        protected TypeToken Root { get; }
        private TypeTokenType MemberType { get; }

        public SerializerGenerator(TypeToken root, TypeTokenType memberType)
        {
            Root = root;
            MemberType = memberType;
        }

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

            bodyParts = bodyParts.TryUnroll();

            var expr = bodyParts.ToExpression();
            var returnType = MakeReturnExpression();
            if (returnType != null)
                expr = Expression.Block(expr, returnType);

            // TODO: See if trying to optimize iterator usages in the generated code outweights the cost of optimizing
            // (It probably doesn't) (except for large files question mark?)
            return expr;
        }

        /// <summary>
        /// Generates a node in the tree representation of the structure.
        /// </summary>
        /// <param name="parent"></param>
        private void GenerateTreeNode(TreeNode parent)
        {
            if (parent.TypeToken.IsArray)
            {
                var loopNode = new LoopTreeNode(parent);

                var elementTypeToken = parent.TypeToken.GetElementTypeToken();
                foreach (var member in elementTypeToken.Members)
                {
                    if (member.MemberType != MemberType || member.IsReadOnly)
                        continue;

                    var node = new TreeNode() {
                        AccessExpression = member.MakeAccess(loopNode.AccessExpression),
                        MemberToken = member,
                        TypeToken = member.TypeToken
                    };

                    GenerateTreeNode(node);

                    loopNode.AddChild(node);
                }

                parent.AddChild(loopNode);
            }
            else
            {
                foreach (var member in parent.TypeToken.Members)
                {
                    if (member.MemberType != MemberType || member.IsReadOnly)
                        continue;

                    var node = new TreeNode() {
                        AccessExpression = member.MakeAccess(parent.AccessExpression),
                        MemberToken = member,
                        TypeToken = member.TypeToken
                    };

                    GenerateTreeNode(node);
                    parent.AddChild(node);
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
