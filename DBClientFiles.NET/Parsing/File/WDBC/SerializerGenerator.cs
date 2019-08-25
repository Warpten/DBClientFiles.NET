﻿using System;
using System.Linq.Expressions;
using DBClientFiles.NET.Parsing.File.Records;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Parsing.Serialization.Generators;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class SerializerGenerator<T> : TypedSerializerGenerator<T>
    {
        public SerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
            Parameters.Add(Expression.Parameter(typeof(IRecordReader)));
            Parameters.Add(Expression.Parameter(typeof(T).MakeByRefType()));
        }

        public override Expression GenerateExpressionReader(TypeToken typeToken, MemberToken memberToken)
        {
            if (typeToken.IsPrimitive)
            {
                var methodCall = typeToken.MakeGenericMethod(_IRecordReader.Read);
                return Expression.Call(RecordReader, methodCall);
            }
            else if (typeToken == typeof(string))    
                return Expression.Call(RecordReader, _IRecordReader.ReadString);
        
            return null;
        }

        protected override Expression RecordReader => Parameters[0];
        protected override Expression FileParser => throw new NotImplementedException();
        protected override Expression Instance => Parameters[1];
    }
}
