using System;
using System.Collections.Generic;

using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;


namespace DBClientFiles.NET.Parsing.Serialization.Generators
{
    internal class TreeNode
    {
        /// <summary>
        /// An expression representing complete access to the node.
        /// 
        /// This is not necessarily a primitive.
        /// </summary>
        public virtual Expr AccessExpression { get; set; }

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
        public List<Expr> ReadExpression { get; } = new List<Expr>();

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

        public virtual Expr ToExpression()
        {
            var blockBody = new List<Expr>();
#if EXPERIMENTAL_GENERATOR
            var variables = new HashSet<ParameterExpression>();
#endif
            // AccessExpression is null on the root node (It's a dummy node)
            // ReadExpression.Count > 1 can only happen if we decided to roll a loop, in which case all elements are identical.
            // ReadExpression.Count = 0 can happen if the type is a value type. Otherwise this is new T[...] or new T().
            if (AccessExpression != null)
            {
                if (ReadExpression.Count > 0)
                    blockBody.Add(Expr.Assign(AccessExpression, ReadExpression[0]));
                else if (TypeToken.IsArray)
                    blockBody.Add(Expr.Assign(AccessExpression,
                        TypeToken.GetElementTypeToken().NewArrayBounds(Expr.Constant(MemberToken.Cardinality))));
                else if (TypeToken.IsClass)
                {
                    if (!TypeToken.HasDefaultConstructor)
                        throw new InvalidOperationException("Missing default constructor for " + TypeToken.Name);

                    blockBody.Add(Expr.Assign(AccessExpression, TypeToken.NewExpression()));
                }
            }

            if (Children.Count > 0)
            {
                // Produce children if there are any
                foreach (var child in Children)
                {
                    var subExpression = child.ToExpression();
#if EXPERIMENTAL_GENERATOR
                    if (subExpression is BlockExpression subBlockExpression)
                    {
                        blockBody.AddRange(subBlockExpression.Expressions);
                        foreach (var subBlockVariable in subBlockExpression.Variables)
                            variables.Add(subBlockVariable);
                    }
                    else
                        blockBody.Add(subExpression);
#else
                    blockBody.Add(subExpression);
#endif
                }

                // Assert that there is at least one node in the block emitted.
                if (blockBody.Count == 0)
                    throw new InvalidOperationException("Empty block");
            }

            // We allow empty blocks if there are no children for primitive types
            if (blockBody.Count == 0)
                return Expr.Empty();

            // If there's only one expression, just return it.
#if EXPERIMENTAL_GENERATOR
            if (blockBody.Count == 1 && variables.Count == 0)
                return blockBody[0];

            return Expression.Block(variables, blockBody);
#else
            if (blockBody.Count == 1)
                return blockBody[0];

            return Expr.Block(blockBody);
#endif
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
        public override Expr AccessExpression => Expr.ArrayAccess(Array.AccessExpression, Iterator);

        /// <summary>
        /// The iteration variable.
        /// </summary>
        public ParameterExpression Iterator { get; }

        /// <summary>
        /// Max number of iterations.
        /// </summary>
        public int IterationCount { get; }

        /// <summary>
        /// Initial value for the loop counter.
        /// </summary>
        public int InitialValue { get; }

        private Expr LoopCondition => Expr.LessThan(Iterator, Expr.Constant(IterationCount));
        private Expr IteratorInitializer => Expr.Assign(Iterator, Expr.Constant(InitialValue));

        public LoopTreeNode(TreeNode parent, ParameterExpression iteratorVariable) : base()
        {
            Iterator = iteratorVariable;
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

        public override Expr ToExpression()
        {
            var loopBody = new List<Expr>();
            if (ReadExpression.Count > 0)
                loopBody.Add(Expr.Assign(AccessExpression, ReadExpression[0]));

            foreach (var child in Children)
                loopBody.Add(child.ToExpression());

            loopBody.Add(Expr.PreIncrementAssign(Iterator));

            var loopExitLabel = Expr.Label();
            var loopCode = Expr.Loop(Expr.IfThenElse(LoopCondition, Expr.Block(loopBody), Expr.Break(loopExitLabel)), loopExitLabel);
            return Expr.Block(new[] { Iterator }, IteratorInitializer, loopCode);
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
            bodyExpression.ReadExpression.Add(bodyExpression.TypeToken.NewArrayBounds(Expr.Constant(IterationCount - InitialValue)));

            // Create N blocks of body
            for (var i = 0; i < IterationCount - InitialValue; ++i)
            {
                var invocationNode = new TreeNode() {
                    // Can't reuse this.AccessExpression because it's bound on the iterator.
                    AccessExpression = Expr.ArrayAccess(Array.AccessExpression, Expr.Constant(i)),
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
                        AccessExpression = Expr.MakeMemberAccess(invocationNode.AccessExpression, childNode.MemberToken.MemberInfo),
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
