using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Runtime;
using DBClientFiles.NET.Parsing.Runtime.Serialization;
using DBClientFiles.NET.Parsing.Shared.Records;
using System;
using System.IO;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    /// <summary>
    /// Generates a method capable of deserializing a given type at runtime from a WDBC stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class RuntimeDeserializer<T> : AbstractRuntimeDeserializer<T>
    {
        public delegate void MethodType(Stream dataStream, in AlignedSequentialRecordReader recordReader, out T instance);

        private IMethodBlock RecordReader { get; }
        private IMethodBlock DataStream { get; }
        
        private readonly Lazy<MethodType> _methodInitializer;
        public MethodType Method => _methodInitializer.Value;

        public RuntimeDeserializer(TypeToken typeToken, TypeTokenType typeTokenType) : base(typeToken, typeTokenType)
        {
            DataStream = new Method.Parameter(typeof(Stream), "dataStream");
            RecordReader = new Method.Parameter(typeof(AlignedSequentialRecordReader).MakeByRefType(), "recordReader");
            Instance = new Method.Parameter(typeof(T).MakeByRefType(), "instance");

            _methodInitializer = new Lazy<MethodType>(() => new Method<MethodType>(CreateBody(),
                DataStream,
                RecordReader,
                Instance
            ).Compile());
        }

        protected override IMethodBlock CreateArrayInitializer(MemberToken memberToken, IMethodBlock assignmentTarget) => null;

        protected override IMethodBlock CreateInstanceInitializer(TypeToken typeToken, IMethodBlock assignmentTarget)
        {
            // TODO: Is there really any benefit in caching the constructed chunks of code?

            if (typeToken.IsPrimitive)
                return new Method.Assignment(assignmentTarget,
                    new Method.MethodCall(RecordReader, typeToken.MakeGenericMethod(AlignedSequentialRecordReader.Methods.Read), DataStream));

            if (typeToken == typeof(string))
                return new Method.Assignment(assignmentTarget,
                    new Method.MethodCall(RecordReader, AlignedSequentialRecordReader.Methods.ReadString, DataStream));

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
    }
}
