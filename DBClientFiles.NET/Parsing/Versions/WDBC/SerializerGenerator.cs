using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;
using DBClientFiles.NET.Parsing.Shared.Records;

namespace DBClientFiles.NET.Parsing.Versions.WDBC
{
    internal sealed class SerializerGenerator<T> : TypedSerializerGenerator<T>
    {
        public delegate void MethodType(IRecordReader recordReader, out T instance);
        private MethodType _methodImpl;

        public MethodType Method {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get  {
                if (_methodImpl == null)
                    _methodImpl = GenerateDeserializer<MethodType>();

                return _methodImpl;
            }
        }

        public SerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
            Parameters.Add(Expression.Parameter(typeof(IRecordReader), "recordReader"));
            Parameters.Add(Expression.Parameter(typeof(T).MakeByRefType(), "instance"));
        }
    
        public override Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            if (typeToken.IsPrimitive)
                return Expression.Call(RecordReader, typeToken.MakeGenericMethod(_IRecordReader.Read));
            else if (typeToken == typeof(string))    
                return Expression.Call(RecordReader, _IRecordReader.ReadString);
        
            return null;
        }

        private Expression RecordReader => Parameters[0];

        protected override Expression Instance => Parameters[1];
    }
}
