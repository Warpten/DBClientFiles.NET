using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDB5.Binding;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal sealed class SerializerGenerator<T> : TypedSerializerGenerator<T, SerializerGenerator<T>.MethodType, int>
    {
        public delegate void MethodType(IRecordReader recordReader, out T instance);

        private readonly IList<MemberMetadata> _memberMetadata;

        /// <summary>
        /// Stores the index column's position in the record. This field is set <b>iff</b> the index table exists.
        /// </summary>
        private readonly int? _indexColumn;

        private ParameterExpression DataStream { get; } = Expr.Parameter(typeof(Stream), "dataStream");

        private ParameterExpression RecordReader { get; } = Expr.Parameter(typeof(IRecordReader), "recordReader");

        protected override ParameterExpression ProducedInstance { get; } = Expr.Parameter(typeof(T).MakeByRefType(), "instance");

        public SerializerGenerator(IBinaryStorageFile storage) : base(storage.Type, storage.Options.TokenType, 0)
        {
            _memberMetadata = storage.FindSegmentHandler<FieldInfoHandler<MemberMetadata>>(SegmentIdentifier.FieldInfo);

            if (storage.Header.IndexTable.Exists)
                _indexColumn = storage.Header.IndexColumn;
        }

        protected override Expression<MethodType> MakeLambda(Expr body)
        {
            return Expr.Lambda<MethodType>(body, new[] {
                DataStream,
                RecordReader,
                ProducedInstance
            });
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

            // if column wasn't found ignore.
            return RELATIONSHIP_TABLE_ENTRY;
        }

        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly MemberMetadata RELATIONSHIP_TABLE_ENTRY = new MemberMetadata();
        static SerializerGenerator() {
            RELATIONSHIP_TABLE_ENTRY.CompressionData.Type = MemberCompressionType.Unknown;
        }

        public override Expr GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            // NOTE: This only works because the generator tries to unroll any loop instead of rolling them
            // Ideally we just call GetMemberInfo with memberToken and we get back the correct metadata
            // unfortunately because of how loops can happen to be (user decides int[2] but each integer is
            // parsed differently), this isn't easily doable. I'm open to ideas.
            var memberMetadata = GetMemberInfo(State++);
            if (memberMetadata == null)
                return null;

            switch (memberMetadata.CompressionData.Type)
            {
                // We have to use immediate readers because all the other ones assume sequential reads
                case MemberCompressionType.Unknown:
                    {
                        // This is used to parse values found in WMOMinimapTexture (@barncastle)
                        // Well ok fair it isn't yet but that's the plan

                        // TODO: use Parameters[1] for this (because it lets us use access blocks!)
                        break;
                    }
                case MemberCompressionType.None:
                case MemberCompressionType.Immediate:
                    if (typeToken.IsPrimitive)
                    {
                        return Expr.Call(RecordReader,
                            typeToken.MakeGenericMethod(_IRecordReader.ReadImmediate),
                            Expr.Constant(memberMetadata.Offset),
                            Expr.Constant(memberMetadata.Size));
                    }
                    else if (typeToken == typeof(string))
                        return Expr.Call(RecordReader,
                            _IRecordReader.ReadStringImmediate,
                            Expr.Constant(memberMetadata.Offset),
                            Expr.Constant(memberMetadata.Size));

                    break;
            }
            
            throw new InvalidOperationException("Unsupported compression type");
        }
    }
}
