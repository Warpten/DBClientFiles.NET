using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal sealed class FieldToken : MemberToken
    {
        private FieldInfo _memberInfo;

        public override TypeToken TypeToken { get; }

        public FieldToken(TypeToken parent, FieldInfo fieldInfo) : base(parent, fieldInfo)
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

        public override Expression MakeChildAccess(IMemberToken token)
        {
            if (!TypeToken.HasChild(token))
                throw new Exception("fixme");

            return null;
        }

        public override TypeTokenType MemberType => TypeTokenType.Field;

        public override bool IsReadOnly => _memberInfo.IsInitOnly;
        public override bool IsArray => _memberInfo.FieldType.IsArray;
        public override int Cardinality { get; }

    }
}
