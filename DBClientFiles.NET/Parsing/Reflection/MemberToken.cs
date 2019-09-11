using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal abstract class MemberToken : IMemberToken, IEquatable<MemberToken>
    {
        public MemberInfo MemberInfo { get; }
        public abstract TypeTokenType MemberType { get; }

        public abstract bool IsArray { get; }
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

        public abstract Expression MakeChildAccess(IMemberToken token);

        public abstract T GetAttribute<T>() where T : Attribute;

        public abstract Expression MakeAccess(Expression parent);

        public bool Equals(MemberToken other)
        {
            return other == this || other.MemberInfo == MemberInfo;
        }
    }
}
