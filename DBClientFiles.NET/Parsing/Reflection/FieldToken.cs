using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DBClientFiles.NET.Attributes;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal sealed class FieldToken : MemberToken
    {
        private FieldInfo _memberInfo;

        public override TypeToken TypeToken { get; }

        public FieldToken(TypeToken parent, FieldInfo fieldInfo, int index) : base(parent, fieldInfo, index)
        {
            _memberInfo = fieldInfo;

            TypeToken = parent.GetChildToken(fieldInfo.FieldType);

            if (IsArray)
            {
                var fbAttr = _memberInfo.GetCustomAttribute<FixedBufferAttribute>();
                if (fbAttr != null)
                {
                    Cardinality = fbAttr.Length;
                }
                else
                {
                    var cardinalityAttribute = _memberInfo.GetCustomAttribute<CardinalityAttribute>();
                    if (cardinalityAttribute != null)
                        Cardinality = cardinalityAttribute.SizeConst;
                    else
                    {
                        var marshalAttr = _memberInfo.GetCustomAttribute<MarshalAsAttribute>();
                        if (marshalAttr != null)
                            Cardinality = marshalAttr.SizeConst;
                    }
                }
            }
        }

        public override T GetAttribute<T>() => _memberInfo.GetCustomAttribute<T>();

        public override TypeTokenType MemberType => TypeTokenType.Field;

        public override bool IsReadOnly => _memberInfo.IsInitOnly;
        public override bool IsArray => _memberInfo.FieldType.IsArray;
        public override int Cardinality { get; }

        public override Expr MakeAccess(Expr parent)
        {
            return Expr.MakeMemberAccess(parent, _memberInfo);
        }
    }
}
