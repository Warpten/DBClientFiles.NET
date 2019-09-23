using System;
using System.Collections.Generic;
using System.Text;
using DBClientFiles.NET.Parsing.Reflection;

namespace DBClientFiles.NET.Parsing.Runtime.Serialization
{
    internal class LoopGenerationState
    {
        public MemberToken MemberToken { get; }
        public UnrollingMode UnrollingMode { get; set; } = UnrollingMode.Unknown;

        public LoopGenerationState(MemberToken memberInfo)
        {
            MemberToken = memberInfo;
        }
    }

    internal enum UnrollingMode
    {
        Always,
        Never,
        Unknown
    }
}
