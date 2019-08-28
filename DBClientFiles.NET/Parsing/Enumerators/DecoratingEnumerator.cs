using DBClientFiles.NET.Parsing.Serialization;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class DecoratingEnumerator<TValue, TSerializer> : Enumerator<TValue, TSerializer>
        where TSerializer : ISerializer<TValue>, new()
    {
        internal Enumerator<TValue, TSerializer> Implementation { get; }


        public DecoratingEnumerator(Enumerator<TValue, TSerializer> impl) : base(impl.Parser)
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
