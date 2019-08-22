using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal sealed class PropertyToken : MemberToken
    {
        private PropertyInfo _propInfo;

        public override TypeToken TypeToken { get; }

        public PropertyToken(TypeToken parent, PropertyInfo propInfo, int index) : base(parent, propInfo, index)
        {
            _propInfo = propInfo;

            TypeToken = parent.GetChildToken(propInfo.PropertyType);

            if (IsArray)
            {
                var fbAttr = _propInfo.GetCustomAttribute<FixedBufferAttribute>();
                if (fbAttr != null)
                {
                    Cardinality = fbAttr.Length;
                }
                else
                {
                    var cardinalityAttribute = _propInfo.GetCustomAttribute<CardinalityAttribute>();
                    if (cardinalityAttribute != null)
                        Cardinality = cardinalityAttribute.SizeConst;
                }
            }
        }

        public override bool IsReadOnly => _propInfo.GetSetMethod() == null;
        public override bool IsArray => _propInfo.PropertyType.IsArray;
        public override int Cardinality { get; }

        public override TypeTokenType MemberType => TypeTokenType.Property;

        public override T GetAttribute<T>()
        {
            return _propInfo.GetCustomAttribute<T>();
        }

        public override Expression MakeChildAccess(IMemberToken token)
        {
            throw new NotImplementedException();
        }

        public override Expression MakeAccess(Expression parent)
        {
            if (parent.Type != DeclaringTypeToken)
                throw new ArgumentException(nameof(parent));
            
            // TODO: This is bad, the backing field name is implementation dependant
            // Should probably get il from getter method body and parse it with cecil to find referenced fields.
            MemberInfo fieldInfo = DeclaringTypeToken.Type.GetField($"<{_propInfo.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null) // Default to property itself if we can't get to the backing field
                fieldInfo = _propInfo;

            return Expression.MakeMemberAccess(parent, fieldInfo);
        }
    }
}
