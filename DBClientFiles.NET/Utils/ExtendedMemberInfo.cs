using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using DBClientFiles.NET.Internals;

namespace DBClientFiles.NET.Utils
{
    /// <summary>
    /// This class wraps around <see cref="MemberInfo"/>. It is internally used to map elements of a defined structure to the elements
    /// declared in the file being parsed.
    /// </summary>
    internal class ExtendedMemberInfo
    {
        public MemberInfo MemberInfo { get; }
        public int Index { get; set; }

        /// <summary>
        /// If this describes a member with underlying members, this only contains the first file member this is mapped to.
        /// </summary>
        public FileMemberInfo MappedTo { get; set; }

        public List<ExtendedMemberInfo> Children { get; } = new List<ExtendedMemberInfo>();
        public ExtendedMemberInfo Parent { get; set; }

        public MemberCompressionType CompressionType => MappedTo.CompressionOptions.CompressionType;

        public Type Type { get; }

        public int Cardinality { get; set; } = 1;

        public ExtendedMemberInfo(MemberInfo memberInfo, int index, ExtendedMemberInfo parent = null)
        {
            Index = index;
            if (parent != null)
                Index += parent.Index;

            Parent = parent;
            MemberInfo = memberInfo;

            switch (memberInfo)
            {
                case PropertyInfo propInfo:
                    Type = propInfo.PropertyType;
                    break;
                case FieldInfo fieldInfo:
                    Type = fieldInfo.FieldType;
                    break;
            }

            var arraySizeAttribute = memberInfo.GetCustomAttribute<CardinalityAttribute>(false);
            if (arraySizeAttribute == null)
            {
                var marshalAttribute = memberInfo.GetCustomAttribute<MarshalAsAttribute>(false);
                if (marshalAttribute != null)
                    Cardinality = marshalAttribute.SizeConst;
            }
            else
                Cardinality = arraySizeAttribute.SizeConst;

            var childIndex = 0;
            foreach (var childMember in Type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                var childInfo = Create(childMember, ref childIndex, this);
                if (childInfo == null)
                    continue;
                
                Children.Add(childInfo);
            }
        }

        public static ExtendedMemberInfo Create(MemberInfo memberInfo, ref int index, ExtendedMemberInfo parent = null)
        {
            ExtendedMemberInfo memberInstance = null;
            switch (memberInfo)
            {
                case PropertyInfo propInfo:
                    if (propInfo.GetSetMethod() == null)
                        return null;
                    memberInstance = new ExtendedMemberInfo(memberInfo, index, parent);
                    break;
                case FieldInfo fieldInfo:
                    if (fieldInfo.IsInitOnly)
                        return null;
                    memberInstance = new ExtendedMemberInfo(memberInfo, index, parent);
                    break;
            }

            if (memberInstance != null)
                index += Math.Max(1, memberInstance.Children.Count);
            return memberInstance;
        }

        public ExtendedMemberExpression MakeMemberAccess(Expression instance)
        {
            return new ExtendedMemberExpression(instance, this);
        }

        public override string ToString()
        {
            if (MappedTo != null)
                return $"{MemberInfo.Name} (#{Index}) => FileFields[{MappedTo.Index}] {{ CompressionType = {MappedTo.CompressionOptions.CompressionType} }}";
            return $"{MemberInfo.Name} (#{Index})";
        }
    }
}
