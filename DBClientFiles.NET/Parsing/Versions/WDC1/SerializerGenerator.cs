using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDC1.Binding;
using System;
using System.IO;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
{
    internal sealed class SerializerGenerator<T> : TypedSerializerGenerator<T, SerializerGenerator<T>.MethodType, int>
    {
        public delegate void MethodType(IRecordReader recordReader, out T instance);

        private FieldInfoHandler<MemberMetadata> FieldInfoBlock { get; }

        /// <summary>
        /// Stores the index column's position in the record. This property is set <b>iff</b> the index table exists.
        /// </summary>
        public int? IndexColumn { get; }

        private ParameterExpression DataStream { get; } = Expression.Parameter(typeof(Stream), "dataStream");

        private ParameterExpression RecordReader { get; } = Expression.Parameter(typeof(IRecordReader), "recordReader");

        protected override ParameterExpression ProducedInstance { get; } = Expression.Parameter(typeof(T).MakeByRefType(), "instance");


        public SerializerGenerator(IBinaryStorageFile storage, FieldInfoHandler<MemberMetadata> fieldInfoBlock) : base(storage.Type, storage.Options.TokenType, 0)
        {
            FieldInfoBlock = fieldInfoBlock;

            if (storage.Header.IndexTable.Exists)
                IndexColumn = storage.Header.IndexColumn;
        }

        protected override Expression<MethodType> MakeLambda(Expression body)
        {
            return Expression.Lambda<MethodType>(body, new[] {
                DataStream,
                RecordReader,
                ProducedInstance
            });
        }

        private MemberMetadata GetMemberInfo(int callIndex)
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
            var memberMetadata = GetMemberInfo(State++);
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
                            Expression.Constant(memberMetadata.Offset),
                            Expression.Constant(memberMetadata.Size));
                    }
                    else if (typeToken == typeof(string))
                        return Expression.Call(RecordReader,
                            _IRecordReader.ReadStringImmediate,
                            Expression.Constant(memberMetadata.Offset),
                            Expression.Constant(memberMetadata.Size));

                    break;
            }

            return null;
        }
    }
}
