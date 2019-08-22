using DBClientFiles.NET.Parsing.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.Parsing.File.Enumerators
{
    internal class DecoratingEnumerator<TValue, TSerializer> : Enumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        internal Enumerator<TValue, TSerializer> Implementation { get; }


        public DecoratingEnumerator(Enumerator<TValue, TSerializer> impl) : base(impl.FileParser)
        {
            Implementation = impl;
        }

        internal override TValue ObtainCurrent()
        {
            return Implementation.ObtainCurrent();
        }

        internal override void ResetIterator()
        {
            Implementation.ResetIterator();
        }
    }
}
