using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.Types;
using DBClientFiles.NET.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.Serialization
{
    internal abstract class BaseMemberSerializer : IMemberSerializer
    {
        public ITypeMember MemberInfo { get; }

        public BaseMemberSerializer(ITypeMember memberInfo)
        {
            MemberInfo = memberInfo;
        }

        /// <summary>
        /// <para>This method is hell.</para>
        /// <para>
        /// For a given node, it recursively visits all of its children, and produces expressions for deserialization.
        /// That's really all you need to know. Deserializing primitives such as integers and string falls on the implementations
        /// of this class (via <see cref="VisitNode(ExtendedMemberExpression, Expression)"/>.
        /// </para>
        /// </summary>
        /// <param name="recordReader">An expression representing an instance of <see cref="IRecordReader"/>.</param>
        /// <param name="rootNode">The node from which we starting work down.</param>
        /// <returns></returns>
        /// <remarks>
        /// Custom types with a constructor taking an instance of <see cref="IRecordReader"/> as their unique argument
        /// cause the entire evaluation to cut off short, and a call to that constructor is emitted instead. This means
        /// that it is the responsability of said constructor to properly initialize any substructure it may contain.
        /// </remarks>
        public IEnumerable<Expression> Visit(Expression recordReader, ExtendedMemberExpression rootNode)
        {
            var memberType = rootNode.MemberInfo.Type;
            var elementType = memberType.IsArray ? memberType.GetElementType() : memberType;

            var memberAccess = rootNode;

            var constructorInfo = elementType.GetConstructor(new[] { typeof(IRecordReader) });
            if (memberType.IsArray)
            {
                if (constructorInfo != null)
                {
                    var arrayExpr = Expression.NewArrayBounds(elementType, Expression.Constant(MemberInfo.Cardinality));
                    yield return Expression.Assign(memberAccess.Expression, arrayExpr);

                    var breakLabelTarget = Expression.Label();

                    var itr = Expression.Variable(typeof(int));
                    var condition = Expression.LessThan(itr, Expression.Constant(MemberInfo.Cardinality));

                    yield return Expression.Loop(Expression.Block(new[] { itr }, new Expression[] {
                        Expression.Assign(itr, Expression.Constant(0)),
                        Expression.IfThenElse(condition,
                            Expression.Assign(
                                Expression.ArrayIndex(memberAccess.Expression, Expression.PostIncrementAssign(itr)),
                                Expression.New(constructorInfo, recordReader)),
                            Expression.Break(breakLabelTarget))
                    }), breakLabelTarget);
                }
                else
                {
                    var nodeInitializer = VisitNode(memberAccess, recordReader);
                    if (nodeInitializer != null)
                        yield return nodeInitializer;

                    var arrayMemberInfo = memberAccess.MemberInfo.Children[0];
                    if (arrayMemberInfo.Children.Count != 0)
                    {
                        var breakLabelTarget = Expression.Label();

                        var itr = Expression.Variable(typeof(int));
                        var arrayBound = Expression.Constant(memberAccess.MemberInfo.Cardinality);
                        var loopTest = Expression.LessThan(itr, arrayBound);

                        var arrayElement = Expression.ArrayAccess(memberAccess.Expression, itr);

                        var loopBody = new List<Expression>();
                        foreach (var childInfo in arrayMemberInfo.Children)
                        {
                            var childAccess = childInfo.MakeMemberAccess(arrayElement);
                            loopBody.AddRange(Visit(recordReader, childAccess));
                        }

                        yield return Expression.Block(new[] { itr },
                            Expression.Assign(itr, Expression.Constant(0)),
                            Expression.Loop(
                                Expression.IfThenElse(loopTest,
                                    Expression.Block(
                                        Expression.Assign(
                                            arrayElement,
                                            New.Expression(arrayMemberInfo.Type)
                                        ),
                                        Expression.Block(loopBody),
                                        Expression.PreIncrementAssign(itr)
                                    ),
                                    Expression.Break(breakLabelTarget)
                                )
                            , breakLabelTarget));
                    }
                }
            }
            else if (constructorInfo != null)
            {
                yield return Expression.Assign(memberAccess.Expression, Expression.New(constructorInfo, recordReader));
            }
            else
            {
                if (memberAccess.MemberInfo.Type.IsClass)
                    yield return Expression.Assign(memberAccess.Expression, Expression.New(memberAccess.MemberInfo.Type));

                var nodeInitializer = VisitNode(memberAccess, recordReader);
                if (nodeInitializer != null)
                    yield return nodeInitializer;

                foreach (var child in memberAccess.MemberInfo.Children)
                {
                    var childAccess = child.MakeMemberAccess(rootNode.Expression);
                    foreach (var subExpression in Visit(recordReader, childAccess))
                        yield return subExpression;
                }
            }
        }

        /// <summary>
        /// This method is invoked on primitive members such as integers, floats, but also on arrays of these.
        /// </summary>
        /// <param name="memberAccess"></param>
        /// <param name="recordReader"></param>
        public abstract Expression VisitNode(ExtendedMemberExpression memberAccess, Expression recordReader);

    }
}


