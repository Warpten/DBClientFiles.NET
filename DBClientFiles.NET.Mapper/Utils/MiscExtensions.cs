using System;
using System.Reflection;
using System.Text;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Definitions.Attributes;

namespace DBClientFiles.NET.Mapper.Utils
{
    internal static class MiscExtensions
    {
        public static Type GetMemberType(this MemberInfo member)
        {
            if (member is PropertyInfo prop)
                return prop.PropertyType;

            if (member is FieldInfo field)
                return field.FieldType;

            if (member is EventInfo ev)
                return ev.EventHandlerType;

            return null;
        }
        
        public static string ToAlias(this Type type)
        {
            var baseType = !type.IsArray ? type : type.GetElementType();
            switch (Type.GetTypeCode(baseType))
            {
                case TypeCode.SByte:
                    return type.IsArray ? "sbyte[]" : "sbyte";
                case TypeCode.Byte:
                    return type.IsArray ? "byte[]" : "byte";
                case TypeCode.Int16:
                    return type.IsArray ? "short[]" : "short";
                case TypeCode.UInt16:
                    return type.IsArray ? "ushort[]" : "ushort";
                case TypeCode.Int32:
                    return type.IsArray ? "int[]" : "int";
                case TypeCode.UInt32:
                    return type.IsArray ? "uint[]" : "uint";
                case TypeCode.Int64:
                    return type.IsArray ? "long[]" : "long";
                case TypeCode.UInt64:
                    return type.IsArray ? "ulong[]" : "ulong";
                case TypeCode.Single:
                    return type.IsArray ? "float[]" : "float";
                case TypeCode.Double:
                    return type.IsArray ? "double[]" : "double";
                case TypeCode.String:
                    return type.IsArray ? "string[]" : "string";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
