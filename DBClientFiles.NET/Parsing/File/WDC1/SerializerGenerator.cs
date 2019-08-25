using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.File.Segments.Handlers;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;
using System;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.File.WDC1
{
    internal sealed class SerializerGenerator<T> : TypedSerializerGenerator<T>
    {
        private int _callIndex;

        private FieldInfoHandler<MemberMetadata> FieldInfoBlock { get; }
        public int? IndexColumn { get; set; }

        public SerializerGenerator(IBinaryStorageFile storage, FieldInfoHandler<MemberMetadata> fieldInfoBlock) : base(storage.Type, storage.Options.TokenType)
        {
            FieldInfoBlock = fieldInfoBlock;

            Parameters.Add(Expression.Parameter(typeof(IRecordReader), "recordReader"));
            Parameters.Add(Expression.Parameter(typeof(T).MakeByRefType(), "instance"));

            _callIndex = 0;
        }

        public MemberMetadata GetMemberInfo(int callIndex)
        {
            if (IndexColumn.HasValue)
            {
                if (callIndex == IndexColumn)
                    return default;
                else if (callIndex > IndexColumn)
                    --callIndex; // Account for the index column
            }

            // TODO: Is improving this needed?
            for (var i = 0; i < FieldInfoBlock.Count; ++i)
            {
                var memberMetadata = FieldInfoBlock[i];
                for (var j = 0; j < memberMetadata.Cardinality; ++j)
                {
                    if (callIndex == 0)
                        return memberMetadata;

                    --callIndex;
                }
            }

            throw new InvalidOperationException();
        }

        public override Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            // NOTE: This only works because the generator tries to unroll any loop instead of rolling them
            var memberMetadata = GetMemberInfo(_callIndex);
            ++_callIndex;
            if (memberMetadata == null)
                return null;

            switch (memberMetadata.CompressionData.Type)
            {
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

            return null;
        }

        protected override Expression FileParser => throw new NotImplementedException();
        protected override Expression RecordReader => Parameters[0];
        protected override Expression Instance => Parameters[1];
    }
}
