using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Versions;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class DecoratingEnumerator<TParser, TValue, TSerializer> : Enumerator<TParser, TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
        where TParser : BinaryFileParser<TValue, TSerializer>
    {
        internal Enumerator<TParser, TValue, TSerializer> Implementation { get; }

        public DecoratingEnumerator(Enumerator<TParser, TValue, TSerializer> impl) : base(impl.Parser)
        {
            Implementation = impl;
        }

        public override void Dispose()
        {
            Implementation.Dispose();
        }

        internal override TValue ObtainCurrent()
        {
            return Implementation.ObtainCurrent();
        }

        public override void Reset()
        {
            base.Reset();

            Implementation.Reset();
        }
    }
}
