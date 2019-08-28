using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions;

namespace DBClientFiles.NET.Parsing.Serialization.Generators
{
    internal class TreeNode
    {
        /// <summary>
        /// An expression representing complete access to the node.
        /// 
        /// This is not necessarily a primitive.
        /// </summary>
        public virtual Expression AccessExpression { get; set; }

        /// <summary>
        /// A <see cref="MemberToken"/>. This is always in sync with actual elements of the structure <b>except</b>
        /// when looking at nodes describing elements of an array, because we technically don't have a MemberToken for that.
        /// So for those, we refer to the array itself.
        /// </summary>
        public MemberToken MemberToken { get; set; }

        /// <summary>
        /// A Type token corresponding to the type of the expression returned by <see cref="AccessExpression"/>.
        /// </summary>
        public TypeToken TypeToken { get; set; }

        /// <summary>
        /// A set of candidate expressions to be used when reading into the expression.
        /// </summary>
        public List<Expression> ReadExpression { get; } = new List<Expression>();

        /// <summary>
        /// Nodes that may be children of this node. Typically, substructure members.
        /// </summary>
        public List<TreeNode> Children { get; } = new List<TreeNode>();

        public TreeNode()
        {
        }

        public virtual TreeNode TryUnroll()
        {
            for (var i = 0; i < Children.Count; ++i)
                Children[i] = Children[i].TryUnroll();

            return this;
        }

        public T AddChild<T>(T child) where T : TreeNode
        {
            Children.Add(child);
            return child;
        }

        public T RemoveChild<T>(T child) where T : TreeNode
        {
            Children.Remove(child);
            return child;
        }

        public virtual void GenerateReadCalls(SerializerGenerator generator)
        {
            foreach (var node in Children)
            {
                if (node.TypeToken != null)
                {
                    var reader = generator.GenerateExpressionReader(node.TypeToken, node.MemberToken);
                    if (reader != null)
                        node.ReadExpression.Add(reader);
                }

                node.GenerateReadCalls(generator);
            }
        }

        public virtual Expression ToExpression()
        {
            var blockBody = new List<Expression>();

            // AccessExpression is null on the root node (It's a dummy node)
            // ReadExpression.Count > 1 can only happen if we decided to roll a loop, in which case all elements are identical.
            // ReadExpression.Count = 0 can happen if the type is a value type. Otherwise this is new T[...] or new T().
            if (AccessExpression != null)
            {
                if (ReadExpression.Count > 0)
                    blockBody.Add(Expression.Assign(AccessExpression, ReadExpression[0]));
                else if (TypeToken.IsArray)
                    blockBody.Add(Expression.Assign(AccessExpression,
                        TypeToken.GetElementTypeToken().NewArrayBounds(Expression.Constant(MemberToken.Cardinality))));
                else if (TypeToken.IsClass)
                {
                    if (!TypeToken.HasDefaultConstructor)
                        throw new InvalidOperationException("Missing default constructor for " + TypeToken.Name);

                    blockBody.Add(Expression.Assign(AccessExpression, TypeToken.NewExpression()));
                }
            }

            if (Children.Count > 0)
            {
                // Produce children if there are any
                foreach (var child in Children)
                    blockBody.Add(child.ToExpression());

                // Assert that there is at least one node in the block emitted.
                if (blockBody.Count == 0)
                    throw new InvalidOperationException("Empty block");
            }

            // We allow empty blocks if there are no children for primitive types
            if (blockBody.Count == 0)
                return Expression.Empty();

            // If there's only one expression, just return it.
            if (blockBody.Count == 1)
                return blockBody[0];

            return Expression.Block(blockBody);
        }
    }

    /// <summary>
    /// An implementation of <see cref="TreeNode"/> that describes a loop.
    /// </summary>
    internal class LoopTreeNode : TreeNode
    {
        /// <summary>
        /// The node corresponding to the array on which this loop operates.
        /// </summary>
        public TreeNode Array { get; }

        /// </inheritDoc>
        /// <remarks>
        /// This is relevant only if the loop is not unrolled.
        /// </remarks>
        public override Expression AccessExpression => Expression.ArrayAccess(Array.AccessExpression, Iterator);

        /// <summary>
        /// The iteration variable.
        /// </summary>
        public Expression Iterator { get; } = Expression.Variable(typeof(int));

        /// <summary>
        /// Max number of iterations.
        /// </summary>
        public int IterationCount { get; }

        /// <summary>
        /// Initial value for the loop counter.
        /// </summary>
        public int InitialValue { get; }

        private Expression LoopCondition => Expression.LessThan(Iterator, Expression.Constant(IterationCount));
        private Expression IteratorInitializer => Expression.Assign(Iterator, Expression.Constant(InitialValue));

        public LoopTreeNode(TreeNode parent) : base()
        {
            Array = parent;
            
            InitialValue = 0;
            IterationCount = parent.MemberToken.Cardinality;

            MemberToken = parent.MemberToken;
            TypeToken = parent.TypeToken.GetElementTypeToken();
        }

        /// <summary>
        /// Evaluates to true if the loop must be unrolled. A loop has to be unrolled if all the non-loop nodes within it have multiple
        /// <b>different</b> deserialization calls.
        /// </summary>
        public bool MustUnroll
        {
            get
            {
                var isRolling = true;

                foreach (var node in Children)
                {
                    // We must not propagate the unroll op to the children
                    if (!(node is LoopTreeNode loopNode))
                    {
                        for (var i = 1; i < node.ReadExpression.Count && isRolling; ++i)
                            isRolling &= ExpressionEqualityComparer.Instance.Equals(node.ReadExpression[0], node.ReadExpression[i]);
                    }
                }

                return !isRolling;
            }
        }

        public override Expression ToExpression()
        {
            var loopBody = new List<Expression>();
            if (ReadExpression.Count > 0)
                loopBody.Add(Expression.Assign(AccessExpression, ReadExpression[0]));

            foreach (var child in Children)
                loopBody.Add(child.ToExpression());

            loopBody.Add(Expression.PreIncrementAssign(Iterator));

            var loopExitLabel = Expression.Label();
            var loopCode = Expression.Loop(Expression.IfThenElse(LoopCondition, Expression.Block(loopBody), Expression.Break(loopExitLabel)), loopExitLabel);
            return Expression.Block(new[] { (ParameterExpression)Iterator }, IteratorInitializer, loopCode);
        }

        public override void GenerateReadCalls(SerializerGenerator generator)
        {
            for (var i = InitialValue; i < IterationCount; ++i)
                base.GenerateReadCalls(generator);
        }

        public override TreeNode TryUnroll()
        {
            if (!MustUnroll)
                return this;

            // Reduce begins now
            var bodyExpression = new TreeNode() {
                TypeToken = Array.TypeToken.GetElementTypeToken(),
                MemberToken = Array.MemberToken,
                AccessExpression = Array.AccessExpression
            };

            // Array initializer added now and only now
            bodyExpression.ReadExpression.Add(bodyExpression.TypeToken.NewArrayBounds(Expression.Constant(IterationCount - InitialValue)));

            // Create N blocks of body
            for (var i = 0; i < IterationCount - InitialValue; ++i)
            {
                var invocationNode = new TreeNode() {
                    // Can't reuse this.AccessExpression because it's bound on the iterator.
                    AccessExpression = Expression.ArrayAccess(Array.AccessExpression, Expression.Constant(i)),
                    MemberToken = Array.MemberToken,
                    TypeToken = Array.TypeToken.GetElementTypeToken()
                };

                // Insert ctor if class
                if (invocationNode.TypeToken.IsClass)
                    invocationNode.ReadExpression.Add(invocationNode.TypeToken.NewExpression());

                // Insert all children
                foreach (var childNode in Children)
                {
                    var newChildNode = new TreeNode() {
                        AccessExpression = Expression.MakeMemberAccess(invocationNode.AccessExpression, childNode.MemberToken.MemberInfo),
                        MemberToken = childNode.MemberToken,
                        TypeToken = childNode.TypeToken
                    };

                    // Now that we created a new block we must try to unroll it if it needs to
                    foreach (var subChildNode in childNode.Children)
                        newChildNode.AddChild(subChildNode.TryUnroll());

                    // If deserialization calls were found select the one corresponding to the currently unrolling iteration.
                    if (childNode.ReadExpression.Count > 0)
                        newChildNode.ReadExpression.Add(childNode.ReadExpression[i]);

                    invocationNode.AddChild(newChildNode);
                }

                bodyExpression.AddChild(invocationNode);
            }

            return bodyExpression;
        }
    }
}
