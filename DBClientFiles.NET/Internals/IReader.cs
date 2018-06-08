using System;
using System.Collections.Generic;
using DBClientFiles.NET.Internals.Binding;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals
{
    internal interface IReader<T> : IDisposable
        where T : class, new()
    {
        bool PrepareMemberInformations();
        void ReadSegments();
        IEnumerable<T> ReadRecords();
        
        CodeGenerator<T> Generator { get; }

        ExtendedMemberInfoCollection MemberStore
        {
            get;
            set;
        }

        U ExtractKey<U>(T instance) where U : struct;
    }
}
