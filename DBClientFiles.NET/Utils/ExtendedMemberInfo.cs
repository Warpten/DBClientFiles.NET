using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using DBClientFiles.NET.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DBClientFiles.NET.Internals;
using static System.Reflection.CustomAttributeExtensions;
using DBClientFiles.NET.Internals.Versions;

namespace DBClientFiles.NET.Utils
{
    internal sealed class ExtendedMemberInfo : MemberInfo
    {
        private static MethodInfo _bitReader = typeof(BinaryReader).GetMethod("ReadBits", new[] { typeof(int) });
        private static Dictionary<TypeCode, MethodInfo> _binaryReaders = new Dictionary<TypeCode, MethodInfo>()
        {
            { TypeCode.UInt64, typeof(BinaryReader).GetMethod("ReadUInt64", Type.EmptyTypes) },
            { TypeCode.UInt32, typeof(BinaryReader).GetMethod("ReadUInt32", Type.EmptyTypes) },
            { TypeCode.UInt16, typeof(BinaryReader).GetMethod("ReadUInt16", Type.EmptyTypes) },
            { TypeCode.Byte,   typeof(BinaryReader).GetMethod("ReadByte", Type.EmptyTypes) },

            { TypeCode.Int64,  typeof(BinaryReader).GetMethod("ReadInt64", Type.EmptyTypes) },
            { TypeCode.Int32,  typeof(BinaryReader).GetMethod("ReadInt32", Type.EmptyTypes) },
            { TypeCode.Int16,  typeof(BinaryReader).GetMethod("ReadInt16", Type.EmptyTypes) },
            { TypeCode.SByte,  typeof(BinaryReader).GetMethod("ReadSByte", Type.EmptyTypes) },

            { TypeCode.String, typeof(BinaryReader).GetMethod("ReadString", Type.EmptyTypes) },
            { TypeCode.Single, typeof(BinaryReader).GetMethod("ReadSingle", Type.EmptyTypes) }
        };

        private readonly MemberInfo _memberInfo;
        public MemberInfo MemberInfo => _memberInfo;

        public MemberCompressionType CompressionType { get; private set; } = MemberCompressionType.None;

        // TODO: Figure out a better name for this member.
        public Type ElementType { get; }

        public bool IsArray => ElementType.IsArray;

        public int ArraySize { get; private set; }

        /// <summary>
        /// Index of the member in the declaring type.
        /// </summary>
        public int MemberIndex { get; }

        private int _bitSize;
        private int BitSize
        {
            get => _bitSize;
            set {
                _bitSize = value;
                if ((value & 7) != 0)
                    BinaryReader = _bitReader;
            }
        }

        public ExtendedMemberInfo(PropertyInfo member, int memberIndex)
        {
            _memberInfo = member;
            MemberIndex = memberIndex;
            ElementType = member.PropertyType;

            InitializeMemberHelpers();

            MemberType = member.MemberType;
            Name = member.Name;
            DeclaringType = member.DeclaringType;
            ReflectedType = member.ReflectedType;
        }

        public ExtendedMemberInfo(FieldInfo member, int memberIndex)
        {
            _memberInfo = member;
            MemberIndex = memberIndex;
            ElementType = member.FieldType;

            InitializeMemberHelpers();

            MemberType = member.MemberType;
            Name = member.Name;
            DeclaringType = member.DeclaringType;
            ReflectedType = member.ReflectedType;
        }

        public static ExtendedMemberInfo Initialize(MemberInfo memberInfo, int memberIndex)
        {
            if (memberInfo is PropertyInfo propInfo)
                return new ExtendedMemberInfo(propInfo, memberIndex);
            else if (memberInfo is FieldInfo fieldInfo)
                return new ExtendedMemberInfo(fieldInfo, memberIndex);
            return null;
        }

        private void InitializeMemberHelpers()
        {
            if (IsArray)
            {
                var marshalAttr = this.GetCustomAttribute<MarshalAsAttribute>();
                if (marshalAttr != null)
                    ArraySize = marshalAttr.SizeConst;
                else
                {
                    var arraySizeAttribute = this.GetCustomAttribute<CardinalityAttribute>();
                    if (arraySizeAttribute != null)
                        ArraySize = arraySizeAttribute.SizeConst;
                    else
                    {
                        var storageAttribute = this.GetCustomAttribute<StoragePresenceAttribute>();
                        if (storageAttribute != null)
                            ArraySize = storageAttribute.SizeConst;
                    }
                }
            }

            var simpleType = IsArray ? ElementType.GetElementType() : ElementType;
            if (_binaryReaders.TryGetValue(Type.GetTypeCode(simpleType), out var binaryReaderMethodInfo))
                BinaryReader = binaryReaderMethodInfo;
        }

        public ExtendedMemberExpression MakeMemberAccess(Expression source)
        {
            return new ExtendedMemberExpression(source, this);
        }

        public MethodInfo BinaryReader { get; private set; }

        public override MemberTypes MemberType { get; }
        public override string Name { get; }
        public override Type DeclaringType { get; }
        public override Type ReflectedType { get; }

        public override object[] GetCustomAttributes(bool inherit) => _memberInfo.GetCustomAttributes(inherit);
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _memberInfo.GetCustomAttributes(attributeType, inherit);
        public override bool IsDefined(Type attributeType, bool inherit) => _memberInfo.IsDefined(attributeType, inherit);
    }
}
