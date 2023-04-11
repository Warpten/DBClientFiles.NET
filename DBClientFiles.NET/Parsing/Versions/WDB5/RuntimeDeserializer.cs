using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DBClientFiles.NET.Parsing.Enums;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Runtime;
using DBClientFiles.NET.Parsing.Runtime.Serialization;
using DBClientFiles.NET.Parsing.Shared.Records;
using DBClientFiles.NET.Parsing.Shared.Segments;
using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Parsing.Versions.WDB5.Binding;

using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Versions.WDB5
{
    internal sealed class RuntimeDeserializer<T> : AbstractRuntimeDeserializer<T>
    {
        public delegate void MethodType(in ByteAlignedRecordReader recordReader, out T instance);

        private IMethodBlock RecordReader { get; }

        private readonly Lazy<MethodType> _methodInitializer;
        public MethodType Method => _methodInitializer.Value;

        private readonly FieldInfoHandler<MemberMetadata> _memberMetadata;
        private readonly int? _indexColumn;

        private int _callIndex;

        public RuntimeDeserializer(IBinaryStorageFile<T> storage) : base(storage.Type, storage.Options.TokenType)
        {
            RecordReader = new Method.Parameter(typeof(ByteAlignedRecordReader).MakeByRefType(), "recordReader");
            Instance = new Method.Parameter(typeof(T).MakeByRefType(), "instance");

            _methodInitializer = new Lazy<MethodType>(() => new Method<MethodType>(CreateBody(),
                RecordReader,
                Instance
            ).Compile());

            _memberMetadata = storage.FindSegmentHandler<FieldInfoHandler<MemberMetadata>>(SegmentIdentifier.FieldInfo);

            if (storage.Header.IndexTable.Exists)
                _indexColumn = storage.Header.IndexColumn;
        }

        protected override IMethodBlock CreateArrayInitializer(MemberToken memberToken, IMethodBlock assignmentTarget) => null;

        protected override IMethodBlock CreateInstanceInitializer(TypeToken typeToken, IMethodBlock assignmentTarget)
        {
            var memberMetadata = GetMemberInfo(_callIndex);
            ++_callIndex;
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
                {
                    if (typeToken.IsPrimitive)
                        return new Method.Assignment(assignmentTarget,
                            new Method.MethodCall(RecordReader, ByteAlignedRecordReader.Methods.Read));

                    if (typeToken == typeof(string))
                        return new Method.Assignment(assignmentTarget,
                            new Method.MethodCall(RecordReader, ByteAlignedRecordReader.Methods.ReadString));

                    if (typeToken == typeof(ReadOnlyMemory<byte>))
                        return new Method.Assignment(assignmentTarget,
                            new Method.MethodCall(RecordReader, ByteAlignedRecordReader.Methods.ReadUTF8));

                    break;
                }
            }

            return null;
        }

        protected override int GetCardinality(MemberToken memberToken) => memberToken.Cardinality;

        protected override UnrollingMode OnLoopGenerationStart(MemberToken memberToken)
        {
            // Fully trust user provided structures (because it's all we can do)
            return UnrollingMode.Never;
        }

        protected override void OnLoopGenerationEnd(MemberToken memberToken)
        {
        }

        protected override UnrollingMode OnLoopGenerationIteration(int iterationIndex, MemberToken memberToken)
        {
            return UnrollingMode.Never;
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
            return UNKNOWN;
        }

        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly MemberMetadata UNKNOWN = new();
        static RuntimeDeserializer()
        {
            UNKNOWN.CompressionData.Type = MemberCompressionType.Unknown;
        }

    }
}
