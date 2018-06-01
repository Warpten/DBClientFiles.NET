using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals;

namespace DBClientFiles.NET.Utils
{
    internal class ExtendedMemberInfoCollection
    {
        public List<ExtendedMemberInfo> Members { get; } = new List<ExtendedMemberInfo>();

        public List<FileMemberInfo> FileMembers { get; } = new List<FileMemberInfo>();
        
        public ExtendedMemberInfoCollection(Type parentType, StorageOptions options)
        {
            var memberIndex = 0;
            foreach (var memberInfo in parentType.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (memberInfo.MemberType != options.MemberType)
                    continue;

                var extendedInfo = ExtendedMemberInfo.Create(memberInfo, ref memberIndex);
                if (extendedInfo != null)
                    Members.Add(extendedInfo);
            }
        }

        public void SetFileMemberInfo(IEnumerable<FileMemberInfo> fileMembers)
        {
            FileMembers.Clear();
            FileMembers.AddRange(fileMembers);
        }

        public int ToCompressionSpecificIndex(int memberIndex)
        {
            //! TODO: Cache this

            var targetMember = FileMembers[memberIndex];
            var targetIndex = 0;
            for (var i = 0; i < memberIndex; ++i)
                if (targetMember.CompressionOptions.CompressionType == FileMembers[i].CompressionOptions.CompressionType)
                    ++targetIndex;
            return targetIndex;
        }

        public void AddFileMemberInfo(int byteSize, int byteOffset)
        {
            FileMembers.Add(new FileMemberInfo
            {
                Offset = byteOffset * 8,
                ByteSize = byteSize,
                Index = FileMembers.Count
            });
        }

        private int RecursiveMemberAssignment(ExtendedMemberInfo memberInfo, int fileIndex, ref int memberOffset)
        {
            if (memberInfo.Index == IndexColumn && !IsIndexStreamed)
                return fileIndex;

            if (memberInfo.Children.Count != 0)
            {
                for (var i = 0; i < memberInfo.Children.Count; ++i)
                    RecursiveMemberAssignment(memberInfo.Children[i], fileIndex + i, ref memberOffset);
                return fileIndex + memberInfo.Children.Count;
            }

            if (FileMembers.Count == 0)
            {
                memberInfo.MappedTo = new FileMemberInfo
                {
                    ByteSize = memberInfo.Type.GetBinarySize(),
                    Offset = memberOffset * 8,
                    Index = fileIndex
                };
                memberOffset += memberInfo.MappedTo.ByteSize * 8;

                return fileIndex + 1;
            }

            if (fileIndex >= FileMembers.Count)
            {
                memberInfo.MappedTo = new FileMemberInfo {
                    ByteSize = memberInfo.Type.GetBinarySize(),
                    Offset = memberOffset * 8,
                    Index = fileIndex,
                };
                memberInfo.MappedTo.BitSize = memberInfo.MappedTo.ByteSize * 8;
                memberInfo.MappedTo.CompressionOptions.CompressionType = MemberCompressionType.RelationshipData;
                memberOffset += memberInfo.MappedTo.ByteSize * 8;

                return fileIndex + 1;
            }

            memberInfo.MappedTo = FileMembers[fileIndex];
            memberOffset += Math.Max(memberInfo.MappedTo.BitSize, memberInfo.MappedTo.ByteSize * 8);

            return fileIndex + 1;
        }

        public void CalculateCardinalities()
        {
            for (var i = 0; i < FileMembers.Count; ++i)
            {
                var currentFileMember = FileMembers[i];

                // Don't need to calculate cardinality for these, we already got the info.
                if (currentFileMember.CompressionOptions.CompressionType == MemberCompressionType.BitpackedPalletArrayData)
                    continue;

                if (currentFileMember.ByteSize > 0)
                    currentFileMember.Cardinality = currentFileMember.BitSize / (8 * currentFileMember.ByteSize);
            }

            // Throw an exception if we mapped to a field that wasn't declared as an array
            for (var i = 0; i < Members.Count; ++i)
            {
                var currentMember = Members[i];
                if (currentMember.MappedTo == null)
                    continue;

                if (currentMember.Type.IsArray && currentMember.MappedTo.Cardinality <= 1)
                    throw new InvalidStructureException($"Field {currentMember.MemberInfo.Name} is declared as an array but maps to a regular type. Is your structure accurate?");

                if (!currentMember.Type.IsArray && currentMember.MappedTo.Cardinality > 1)
                    throw new InvalidStructureException($"Field {currentMember.MemberInfo.Name} is declared as a simple type but maps to an array. Is your structure accurate?");
            }
        }

        public void MapMembers()
        {
            var fileCursor = 0;
            var memberOffset = 0;
            foreach (var memberInfo in Members)
                fileCursor = RecursiveMemberAssignment(memberInfo, fileCursor, ref memberOffset);
        }

        public int IndexColumn { get; set; } = 0;
        public bool IsIndexStreamed { get; set; } = true;

        public IEnumerable<int> GetBlockLengths(MemberCompressionType compressionType)
        {
            return FileMembers.Where(f => f.CompressionOptions.CompressionType == compressionType).Select(f => f.CompressionOptions.CompressedDataSize);
        }

        public ExtendedMemberInfo IndexMember
        {
            get
            {
                var indexMember = Members.FirstOrDefault(m => m.MemberInfo.IsDefined(typeof(IndexAttribute), false));
                if (indexMember == null)
                    return Members[0];
                return indexMember;
            }
        }
    }
}