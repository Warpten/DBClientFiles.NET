using System;

namespace DBClientFiles.NET.Parsing.Enums
{
    internal static class MemberCompressionTypeExtensions
    {
        internal enum MemberCompressionGroup
        {
            Inlined,
            Pallet,
            RelationshipData,
            CommonData
        }

        public static MemberCompressionGroup GetCompressionGroup(this MemberCompressionType type)
        {
            switch (type)
            {
                case MemberCompressionType.None:
                case MemberCompressionType.SignedImmediate:
                case MemberCompressionType.Immediate:
                    return MemberCompressionGroup.Inlined;
                case MemberCompressionType.BitpackedPalletArrayData:
                case MemberCompressionType.BitpackedPalletData:
                    return MemberCompressionGroup.Pallet;
                case MemberCompressionType.RelationshipData:
                    return MemberCompressionGroup.RelationshipData;
                case MemberCompressionType.CommonData:
                    return MemberCompressionGroup.CommonData;
            }

            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
