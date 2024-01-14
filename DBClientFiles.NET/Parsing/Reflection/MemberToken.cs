using System;
using System.Reflection;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;
using DBClientFiles.NET.Parsing.Runtime;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal abstract class MemberToken : IMemberToken, IEquatable<MemberToken>
    {
        /// <summary>
        /// Informations on this member.
        /// </summary>
        public MemberInfo MemberInfo { get; }

        /// <summary>
        /// The type of member.
        /// </summary>
        public abstract TypeTokenKind MemberType { get; }

        /// <summary>
        /// Is the type described by this member an array?
        /// </summary>
        public abstract bool IsArray { get; }

        /// <summary>
        /// The cardinality of the member described.
        /// </summary>
        public abstract int Cardinality { get; }

        public int Index { get; }

        public abstract TypeToken TypeToken { get; }

        /// <summary>
        /// A <see cref="Reflection.TypeToken"/> for the declaring type of this member.
        /// </summary>
        public TypeToken DeclaringTypeToken { get; }

        protected MemberToken(TypeToken parent, MemberInfo memberInfo, int index)
        {
            DeclaringTypeToken = parent;
            MemberInfo = memberInfo;
            Index = index;
        }

        public abstract bool IsReadOnly { get; }

        public abstract T GetAttribute<T>() where T : Attribute;

        public abstract Expr MakeAccess(Expr parent);
        public abstract IExpression MakeArrayAccess(IExpression arg);

        public bool Equals(MemberToken other)
        {
            return other == this || other.MemberInfo == MemberInfo;
        }
    }
}
