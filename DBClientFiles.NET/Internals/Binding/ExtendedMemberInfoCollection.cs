using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Binding
{
    public class ExtendedMemberInfoCollection
    {
        public List<ExtendedMemberInfo> Members { get; } = new List<ExtendedMemberInfo>();

        public List<FileMemberInfo> FileMembers { get; } = new List<FileMemberInfo>();

        internal ExtendedMemberInfoCollection(Type parentType, StorageOptions options)
        {
            if (parentType == null)
                return;

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

        internal int DeclaredMemberCount(bool includeArrays = true, bool deepTraverse = true)
        {
            var memberCount = 0;
            for (var i = 0; i < Members.Count; ++i)
                memberCount += GetMemberCount(Members[i], deepTraverse, includeArrays);

            return memberCount;
        }

        internal static int GetMemberCount(ExtendedMemberInfo memberInfo, bool deepTraverse = true, bool includeArrays = true)
        {
            if (memberInfo.Children.Count == 0)
                return includeArrays ? memberInfo.Cardinality : 1;

            var memberCount = 0;
            for (var i = 0; i < memberInfo.Children.Count; ++i)
                memberCount += GetMemberCount(memberInfo.Children[i], deepTraverse, includeArrays);
            return memberCount;
        }

        public ExtendedMemberInfo IndexMember
        {
            get
            {
                if (IndexColumn == -1)
                    throw new InvalidOperationException("Index column not bound");

                for (var i = 0; i < Members.Count; ++i)
                {
                    var memberInfo = Members[i];
                    if (memberInfo.MemberInfo.IsDefined(typeof(IndexAttribute), false))
                        return memberInfo;

                    if (memberInfo.MappedTo != null && memberInfo.MappedTo.Index == IndexColumn)
                        return memberInfo;
                }

                throw new InvalidOperationException("Unable to find index");
            }
        }

        internal void SetFileMemberInfo(IEnumerable<FileMemberInfo> fileMembers)
        {
            FileMembers.Clear();
            FileMembers.AddRange(fileMembers);
        }

        internal void AddFileMemberInfo(BinaryReader reader)
        {
            var instance = new FileMemberInfo();
            instance.Initialize(reader);
            instance.Index = FileMembers.Count;
            FileMembers.Add(instance);
        }

        internal int RecursiveMemberAssignment(ExtendedMemberInfo memberInfo, int fileIndex, ref int memberOffset)
        {
            // Don't map the index if it's contained in the index table - Mapping defines how we deserialize later on
            // (an unmapped member is skipped)
            if (fileIndex == IndexColumn && HasIndexTable && memberInfo.Index == IndexColumn)
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
                    Offset = memberOffset,
                    Index = fileIndex,
                    Cardinality = memberInfo.Cardinality
                };

                memberOffset += memberInfo.MappedTo.ByteSize * 8 * memberInfo.MappedTo.Cardinality;

                return fileIndex + 1;
            }

            if (fileIndex >= FileMembers.Count)
            {
                memberInfo.MappedTo = new FileMemberInfo
                {
                    ByteSize = memberInfo.Type.GetBinarySize(),
                    Offset = memberOffset,
                    Index = fileIndex,
                    Cardinality = memberInfo.Cardinality,
                    CompressionType = MemberCompressionType.RelationshipData
                };

                memberOffset += memberInfo.MappedTo.ByteSize * 8 * memberInfo.MappedTo.Cardinality;

                return fileIndex + 1;
            }

            memberInfo.MappedTo = FileMembers[fileIndex];
            memberInfo.Cardinality = memberInfo.MappedTo.Cardinality;
            memberOffset += Math.Max(memberInfo.MappedTo.BitSize, memberInfo.MappedTo.ByteSize * 8);

            return fileIndex + 1;
        }

        internal void CalculateCardinalities()
        {
            for (var i = 0; i < FileMembers.Count; ++i)
            {
                var currentFileMember = FileMembers[i];

                // Don't need to calculate cardinality for these, we already got the info.
                if (currentFileMember.CompressionType == MemberCompressionType.BitpackedPalletArrayData)
                    continue;

                if (currentFileMember.ByteSize > 0)
                    currentFileMember.Cardinality = currentFileMember.BitSize / (8 * currentFileMember.ByteSize);

                // Calculate cardinality from FileMember offsets
                if (currentFileMember.Cardinality <= 1 && i < FileMembers.Count - 1)
                    currentFileMember.Cardinality = ((FileMembers[i + 1].Offset - currentFileMember.Offset) / 8) / currentFileMember.ByteSize;
            }

            // Throw an exception if we mapped to a field that wasn't declared as an array
            for (var i = 0; i < Members.Count; ++i)
            {
                var currentMember = Members[i];
                if (currentMember.MappedTo == null)
                    continue;

                if (currentMember.Type.IsArray && currentMember.MappedTo.Cardinality <= 1)
                    throw new InvalidStructureException($"Member {currentMember.MemberInfo.Name} of {currentMember.MemberInfo.DeclaringType.FullName} is declared as an array but maps to a regular type. Is your structure accurate?");

                if (!currentMember.Type.IsArray && currentMember.MappedTo.Cardinality > 1)
                    throw new InvalidStructureException($"Member {currentMember.MemberInfo.Name} of {currentMember.MemberInfo.DeclaringType.FullName} is declared as a simple type but maps to an array. Is your structure accurate?");
            }

            // And finally, set category specific indices
            for (var i = 1; i < FileMembers.Count; ++i)
            {
                var currentFileMember = FileMembers[i];
                for (var j = i - 1; j >= 0; --j)
                {
                    var currentCategory = currentFileMember.CompressionType;
                    var otherCategory = FileMembers[j].CompressionType;

                    bool incrementCategoryIndex;
                    if (currentCategory == MemberCompressionType.BitpackedPalletArrayData || currentCategory == MemberCompressionType.BitpackedPalletData)
                        incrementCategoryIndex = (otherCategory == MemberCompressionType.BitpackedPalletArrayData || otherCategory == MemberCompressionType.BitpackedPalletData);
                    else
                        incrementCategoryIndex = otherCategory == currentCategory;

                    if (incrementCategoryIndex)
                    {
                        currentFileMember.CategoryIndex = FileMembers[j].CategoryIndex + 1;
                        break;
                    }
                }
            }
        }

        internal void MapMembers()
        {
            var fileCursor = 0;
            var memberOffset = 0;
            foreach (var memberInfo in Members)
                fileCursor = RecursiveMemberAssignment(memberInfo, fileCursor, ref memberOffset);
        }

        public int IndexColumn { get; internal set; } = -1;
        public bool HasIndexTable { get; internal set; } = false;

        internal IEnumerable<int> GetBlockLengths(MemberCompressionType compressionType)
        {
            return FileMembers.Where(f => f.CompressionType == compressionType).Select(f => f.CompressedDataSize);
        }

        internal IEnumerable<int> GetBlockLengths(Func<FileMemberInfo, bool> compressionType)
        {
            return FileMembers.Where(compressionType).Select(f => f.CompressedDataSize);
        }
    }
}