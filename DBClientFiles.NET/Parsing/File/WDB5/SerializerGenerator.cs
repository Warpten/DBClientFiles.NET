using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDB5
{
    internal sealed class SerializerGenerator<T, TMethod> : TypedSerializerGenerator<T, TMethod> where TMethod : Delegate
    {
        private IList<MemberMetadata> _memberMetadata;
        private int _callIndex;

        private int? _indexColumn;

        public SerializerGenerator(TypeToken root, TypeTokenType memberType, IList<MemberMetadata> memberMetadata) : base(root, memberType)
        {
            _memberMetadata = memberMetadata;
        }

        public void SetIndexColumn(int indexColumn)
        {
            _indexColumn = indexColumn;
        }

        public MemberMetadata GetMemberInfo(int callIndex)
        {
            if (_indexColumn.HasValue)
            {
                // WDB5 doesn't list the index column if it's part of the index table
                // So we have to jump through some hoops to get it to run properly
                
                if (callIndex == _indexColumn)
                    return default;
                else if (callIndex > _indexColumn)
                    --callIndex; // Account for the index column
            }

            // TODO: Is improving this needed?
            for (var i = 0; i < _memberMetadata.Count; ++i)
            {
                var memberMetadata = _memberMetadata[i];
                for (var j = 0; j < memberMetadata.Cardinality; ++j)
                {
                    if (callIndex == 0)
                        return memberMetadata;

                    --callIndex;
                }
            }

            return RELATIONSHIP_TABLE_ENTRY;
        }

        private static MemberMetadata RELATIONSHIP_TABLE_ENTRY = new MemberMetadata();
        static SerializerGenerator()
        {
            RELATIONSHIP_TABLE_ENTRY.CompressionData.Type = MemberCompressionType.RelationshipData;
        }

        public override Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            var memberMetadata = GetMemberInfo(_callIndex++);
            if (memberMetadata == null)
                return null;

            switch (memberMetadata.CompressionData.Type)
            {
                case MemberCompressionType.RelationshipData:
                    {
                        // Uhhhh.... Big yikes?
                        return null;
                    }
                // We have to use immediate readers because all the other ones assume sequential reads
                case MemberCompressionType.None:
                case MemberCompressionType.Immediate:
                    if (typeToken.IsPrimitive)
                    {
                        return Expression.Call(RecordReader,
                            typeToken.MakeGenericMethod(_IRecordReader.ReadImmediate),
                            Expression.Constant(memberMetadata.CompressionData.Offset),
                            Expression.Constant(memberMetadata.CompressionData.Size));
                    }
                    else if (typeToken == typeof(string))
                        return Expression.Call(RecordReader,
                            _IRecordReader.ReadStringImmediate,
                            Expression.Constant(memberMetadata.CompressionData.Offset),
                            Expression.Constant(memberMetadata.CompressionData.Size));

                    break;
            }
            
            throw new InvalidOperationException("Unsupported compression type");
        }
    }
}
