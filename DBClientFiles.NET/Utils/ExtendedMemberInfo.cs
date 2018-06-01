using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using DBClientFiles.NET.Internals;
using System.Diagnostics;
using System.Linq;
using DBClientFiles.NET.Collections;

namespace DBClientFiles.NET.Utils
{
    internal class FileMemberInfo
    {
        public int ByteSize { get; set; }
        public int BitSize  { get; set; }
        public int Offset   { get; set; }
        public int Index    { get; set; }

        public int Cardinality { get; set; }

        public CompressionInfo CompressionOptions;

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct BitpackedInfo
        {
            [FieldOffset(0)] public int OffsetBits;
            [FieldOffset(4)] public int SizeBits;
            [FieldOffset(8)] public int Flags;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct CommonDataInfo
        {
            [FieldOffset(0)] public int DefaultValue;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct PalletInfo
        {
            [FieldOffset(0)] public int OffsetBits;
            [FieldOffset(4)] public int SizeBits;
            [FieldOffset(8)] public int ArraySize;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct CompressionInfo
        {
            [FieldOffset(0)] public int CompressedDataSize;
            [FieldOffset(4)] public MemberCompressionType CompressionType;
            [FieldOffset(8)] public BitpackedInfo Bitpacked;
            [FieldOffset(8)] public CommonDataInfo CommonData;
            [FieldOffset(8)] public PalletInfo Pallet;
        }

        public unsafe T GetDefaultValue<T>() where T : struct
        {
            Debug.Assert(CompressionOptions.CompressionType == MemberCompressionType.CommonData);

            Span<int> asInt = stackalloc int[1];
            asInt[0] = CompressionOptions.CommonData.DefaultValue;
            return MemoryMarshal.Cast<int, T>(asInt)[0];
        }
    }

    internal class StructureMemberInfo
    {
        public MemberInfo MemberInfo { get; }

        /// <summary>
        /// If this describes a member with underlying members, this only contains the first file member this is mapped to.
        /// </summary>
        public FileMemberInfo MappedTo { get; set; }

        public List<StructureMemberInfo> Children { get; } = new List<StructureMemberInfo>();
        public StructureMemberInfo Parent { get; set; }

        public MemberCompressionType CompressionType => MappedTo.CompressionOptions.CompressionType;

        public Type Type { get; }

        public int Cardinality { get; set; } = 1;

        public StructureMemberInfo(MemberInfo memberInfo, StructureMemberInfo parent = null)
        {
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

            foreach (var childMember in Type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                var childInfo = Create(childMember, this);
                if (childInfo == null)
                    continue;
                
                Children.Add(childInfo);
            }
        }

        public static StructureMemberInfo Create(MemberInfo memberInfo, StructureMemberInfo parent = null)
        {
            switch (memberInfo)
            {
                case PropertyInfo propInfo:
                    if (propInfo.GetSetMethod() == null)
                        return null;
                    return new StructureMemberInfo(memberInfo, parent);
                case FieldInfo fieldInfo:
                    if (fieldInfo.IsInitOnly)
                        return null;
                    return new StructureMemberInfo(memberInfo, parent);
            }

            return null;
        }

        public ExtendedMemberExpression MakeMemberAccess(Expression instance)
        {
            return new ExtendedMemberExpression(instance, this);
        }
    }

    internal class ExtendedMemberInfoCollection
    {
        private List<StructureMemberInfo> _members = new List<StructureMemberInfo>();

        private List<FileMemberInfo> _fileMembers = new List<FileMemberInfo>();

        public int FileMemberCount => _fileMembers.Count;
        public int StructureMemberCount => _members.Count;

        public ExtendedMemberInfoCollection(Type parentType, StorageOptions options)
        {
            foreach (var memberInfo in parentType.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (memberInfo.MemberType != options.MemberType)
                    continue;

                var extendedInfo = StructureMemberInfo.Create(memberInfo);
                if (extendedInfo != null)
                    _members.Add(extendedInfo);
            }
        }

        public void AddFileMemberInfo(int byteSize, int byteOffset)
        {
            _fileMembers.Add(new FileMemberInfo()
            {
                Offset = byteOffset * 8,
                ByteSize = byteSize,
                Index = _fileMembers.Count
            });
        }

        private int RecursiveMap(StructureMemberInfo memberInfo, int fileIndex, ref int memberOffset)
        {
            if (memberInfo.Children.Count != 0)
            {
                for (var i = 0; i < memberInfo.Children.Count; ++i)
                    RecursiveMap(memberInfo.Children[i], fileIndex + i, ref memberOffset);
                return fileIndex + memberInfo.Children.Count;
            }

            if (_fileMembers.Count == 0)
            {
                memberInfo.MappedTo = new FileMemberInfo()
                {
                    ByteSize = memberInfo.Type.GetBinarySize(),
                    Offset = memberOffset * 8,
                    Index = fileIndex
                };
                memberOffset += memberInfo.MappedTo.ByteSize * 8;
                return fileIndex + 1;
            }

            if (fileIndex >= _fileMembers.Count)
            {
                memberInfo.MappedTo = new FileMemberInfo()
                {
                    ByteSize = memberInfo.Type.GetBinarySize(),
                    Offset = memberOffset * 8,
                    Index = fileIndex,
                };
                memberInfo.MappedTo.CompressionOptions.CompressionType = MemberCompressionType.RelationshipData;
                memberOffset += memberInfo.MappedTo.ByteSize * 8;
                return fileIndex + 1;
            }

            memberInfo.MappedTo = _fileMembers[fileIndex];
            memberOffset += memberInfo.MappedTo.ByteSize * 8;
            return fileIndex + 1;
        }

        public void Map()
        {
            var fileCursor = 0;
            var memberOffset = 0;
            foreach (var memberInfo in _members)
                fileCursor = RecursiveMap(memberInfo, fileCursor, ref memberOffset);
        }

        public FileMemberInfo GetFileMember(int index)           => _fileMembers[index];
        public StructureMemberInfo GetStructureMember(int index) => _members[index];

        public StructureMemberInfo IndexMember
        {
            get
            {
                var indexMember = _members.FirstOrDefault(m => m.MemberInfo.IsDefined(typeof(IndexAttribute), false));
                if (indexMember == null)
                    return _members[0];
                return indexMember;
            }
        }
    }

    /// <summary>
    /// A convenient class that is used to store metadata information about fields in the record.
    /// </summary>
    internal sealed class ExtendedMemberInfo
    {
        public MemberInfo MemberInfo { get; }
        public MemberCompressionType CompressionType { get; set; } = MemberCompressionType.None;
        
        /// <summary>
        /// The type of the target property, as declared in the structure.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Returns true if the associated member has a getter.
        /// </summary>
        /// <remarks>Always <code>false</code> if <see cref="MemberType"/> is not <see cref="MemberTypes.Property"/>.</remarks>
        public bool HasGetter { get; }

        /// <summary>
        /// Returns true if the associated member has a setter.
        /// </summary>
        /// <remarks>Always <code>false</code> if <see cref="MemberType"/> is not <see cref="MemberTypes.Property"/>.</remarks>
        public bool HasSetter { get; }

        /// <summary>
        /// Returns true if the associated member is read-only.
        /// </summary>
        public bool IsInitOnly { get; }

        /// <summary>
        /// The size of the array, assuming it is one; 0 otherwise.
        /// </summary>
        public int Cardinality { get; set; }

        /// <summary>
        /// Index of the member in the declaring type.
        /// </summary>
        public int MemberIndex { get; }

        /// <summary>
        /// Indicates wether or not the member is flagged as signed in file metadata.
        /// </summary>
        public bool IsSigned { get; set; }

        /// <summary>
        /// The default value as specified in file metadata, if any. This could also be a float.
        /// </summary>
        public ReadOnlyMemory<byte> DefaultValue { private get; set; }

        /// <summary>
        /// Offset, in bits, of this member in the record.
        /// </summary>
        public int Offset { get; set; }
        
        /// <summary>
        /// If <see cref="CompressionType"/> is either of <see cref="MemberCompressionType.BitpackedPalletArrayData"/>,
        /// <see cref="MemberCompressionType.BitpackedPalletData"/> or <see cref="MemberCompressionType.CommonData"/>,
        /// this is the offset at which the beginning of this column's block is located.
        /// </summary>
        public int CompressedDataOffset { get; set; }

        /// <summary>
        /// The bit size of this field - this is used only if <see cref="CompressionType"/>
        /// is set to <see cref="MemberCompressionType.Immediate"/>, <see cref="MemberCompressionType.BitpackedPalletData"/>
        /// or <see cref="MemberCompressionType.BitpackedPalletArrayData"/>.
        /// </summary>
        public int BitSize { get; set; }
        
        public int ByteSize { get; set; }

        public ExtendedMemberInfo(PropertyInfo member, int memberIndex)
        {
            MemberInfo = member;
            MemberIndex = memberIndex;
            Type = member.PropertyType;

            IsInitOnly = false;
            HasGetter = member.GetGetMethod() != null;
            HasSetter = member.GetSetMethod() != null;

            InitializeMemberHelpers();
        }

        public ExtendedMemberInfo(FieldInfo member, int memberIndex)
        {
            MemberInfo = member;
            MemberIndex = memberIndex;
            Type = member.FieldType;

            IsInitOnly = member.IsInitOnly;

            InitializeMemberHelpers();
        }

        public static ExtendedMemberInfo Initialize(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propInfo)
                return new ExtendedMemberInfo(propInfo, 0);
            else if (memberInfo is FieldInfo fieldInfo)
                return new ExtendedMemberInfo(fieldInfo, 0);
            return null;
        }

        private void InitializeMemberHelpers()
        {
            if (Type.IsArray)
            {
                var marshalAttr = MemberInfo.GetCustomAttribute<MarshalAsAttribute>();
                if (marshalAttr != null)
                    Cardinality = marshalAttr.SizeConst;
                else
                {
                    var arraySizeAttribute = MemberInfo.GetCustomAttribute<CardinalityAttribute>();
                    if (arraySizeAttribute != null)
                        Cardinality = arraySizeAttribute.SizeConst;
                    else
                    {
                        var storageAttribute = MemberInfo.GetCustomAttribute<StoragePresenceAttribute>();
                        if (storageAttribute != null)
                            Cardinality = storageAttribute.SizeConst;
                    }
                }
            }
        }

        public MemberTypes MemberType => MemberInfo.MemberType;
        public string Name => MemberInfo.Name;

        public bool IsDefined(Type attributeType, bool inherit = false) => MemberInfo.IsDefined(attributeType, inherit);

        public T GetDefaultValue<T>() where T : struct
        {
            var typeMemory = MemoryMarshal.Cast<byte, T>(DefaultValue.Span);
            return typeMemory[0];
        }
    }
}
