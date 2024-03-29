﻿using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Runtime.Serialization;
using DBClientFiles.NET.Parsing.Serialization.Generators;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDC1.Binding;
using System;
using System.Diagnostics;
using System.IO;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Versions.WDC1
{
    internal sealed class SerializerGenerator<T> : TypedSerializerGenerator<T, SerializerGenerator<T>.MethodType, int>
    {
        public delegate void MethodType(IRecordReader recordReader, out T instance);

        private readonly RecordKeyAccessor<T> _keyAccessor;

        private FieldInfoHandler<MemberMetadata> FieldInfoBlock { get; }

        /// <summary>
        /// Stores the index column's position in the record. This property is set <b>iff</b> the index table exists.
        /// </summary>
        public int? IndexColumn { get; }

        private ParameterExpression DataStream = Expr.Parameter(typeof(Stream), "dataStream");
        private ParameterExpression RecordReader = Expr.Parameter(typeof(IRecordReader), "recordReader");

        protected override ParameterExpression ProducedInstance { get; } = Expr.Parameter(typeof(T).MakeByRefType(), "instance");

        public SerializerGenerator(IBinaryStorageFile storage, FieldInfoHandler<MemberMetadata> fieldInfoBlock) : base(storage.Type, storage.Options.TokenType, 0)
        {
            FieldInfoBlock = fieldInfoBlock;

            _keyAccessor = new (storage.Type, storage.Header.IndexColumn, storage.Options.TokenType);

            if (storage.Header.IndexTable.Exists)
                IndexColumn = storage.Header.IndexColumn;
        }

        protected override Expression<MethodType> MakeLambda(Expr body)
        {
            return Expr.Lambda<MethodType>(body, new[] {
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

        public override Expr GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            // NOTE: This only works because the generator tries to unroll any loop instead of rolling them
            var memberMetadata = GetMemberInfo(State++);
            if (memberMetadata == null)
                return null;

            switch (memberMetadata.CompressionData.Type)
            {
                // We have to use immediate readers because all the other ones assume sequential reads
                case MemberCompressionType.SignedImmediate:
                    Debug.Assert(typeToken.IsSigned);
                    goto case MemberCompressionType.Immediate;
                case MemberCompressionType.None:
                case MemberCompressionType.Immediate:
                {
                    if (typeToken.IsPrimitive)
                        return Expr.Call(RecordReader,
                            typeToken.MakeGenericMethod(IRecordReader.Methods.ReadImmediate),
                            Expr.Constant(memberMetadata.Offset),
                            Expr.Constant(memberMetadata.Size));

                    if (typeToken == typeof(string))
                        return Expr.Call(RecordReader,
                            IRecordReader.Methods.ReadStringImmediate,
                            Expr.Constant(memberMetadata.Offset),
                            Expr.Constant(memberMetadata.Size));

                    if (typeToken == typeof(ReadOnlyMemory<byte>))
                        return Expr.Call(RecordReader,
                            IRecordReader.Methods.ReadUTF8Immediate,
                            Expr.Constant(memberMetadata.Offset),
                            Expr.Constant(memberMetadata.Size));

                    break;
                }
                case MemberCompressionType.BitpackedPalletData:
                    return Expr.Call(RecordReader, 
                        typeToken.MakeGenericMethod(IRecordReader.Methods.ReadPallet), 
                        Expr.Constant(memberMetadata.Offset), 
                        Expr.Constant(memberMetadata.Size));
                case MemberCompressionType.BitpackedPalletArrayData:
                    return Expr.Call(RecordReader,
                        typeToken.MakeGenericMethod(IRecordReader.Methods.ReadPalletArray),
                        Expr.Constant(memberMetadata.Offset),
                        Expr.Constant(memberMetadata.Size),
                        Expr.Constant(memberMetadata.Cardinality));
                case MemberCompressionType.CommonData:
                    return Expr.Call(RecordReader,
                        typeToken.MakeGenericMethod(IRecordReader.Methods.ReadCommon),
                        Expr.Constant(memberMetadata.CompressionData.Index),
                        _keyAccessor.AccessIndex(ProducedInstance),
                        Expr.Constant(memberMetadata.DefaultValue.Value));
            }

            return null;    
        }
    }
}
