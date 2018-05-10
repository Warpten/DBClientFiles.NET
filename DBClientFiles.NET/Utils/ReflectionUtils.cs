using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.IO;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Utils
{
    internal static class ReflectionUtils
    {
        public static Expression MakeMemberAccess(this MemberInfo member, Expression target)
        {
            return Expression.MakeMemberAccess(target, member);
        }

        /// <summary>
        /// A convenience wrapper around <see cref="FieldInfo.FieldType"/> or <see cref="PropertyInfo.PropertyType"/> depending on the underlying <see cref="MemberInfo.MemberType"/>.
        /// </summary>
        /// <param name="member"></param>
        /// <returns>Either <see cref="FieldInfo.FieldType"/> or <see cref="PropertyInfo.PropertyType"/>.</returns>
        public static Type GetMemberType(this MemberInfo member)
        {
            if (member is FieldInfo fieldInfo)
                return fieldInfo.FieldType;

            if (member is PropertyInfo propInfo)
                return propInfo.PropertyType;

            return null;
        }

        public static MethodInfo GetReaderMethod(this Type type)
        {
            if (_binaryReaders.TryGetValue(Type.GetTypeCode(type), out var methodInfo))
                return methodInfo;

            return null;
        }

        /// <summary>
        /// A convenience wrapper that determines a compile-time specification of the size of an array.
        ///
        /// This method checks for <see cref="MarshalAsAttribute"/>, <see cref="CardinalityAttribute"/> or <see cref="StoragePresenceAttribute"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetArraySize(this MemberInfo type)
        {
            var marshalAttr = type.GetCustomAttribute<MarshalAsAttribute>();
            if (marshalAttr != null)
                return marshalAttr.SizeConst;

            var arraySizeAttribute = type.GetCustomAttribute<CardinalityAttribute>();
            if (arraySizeAttribute != null)
                return arraySizeAttribute.SizeConst;

            var storageAttribute = type.GetCustomAttribute<StoragePresenceAttribute>();
            if (storageAttribute != null)
                return storageAttribute.SizeConst;

            return 0;
        }

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
    }
}
