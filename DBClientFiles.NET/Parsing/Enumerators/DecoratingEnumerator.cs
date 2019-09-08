using DBClientFiles.NET.Parsing.Versions;

namespace DBClientFiles.NET.Parsing.Enumerators
{
    internal class DecoratingEnumerator<TParser, TValue> : Enumerator<TParser, TValue>
        where TParser : BinaryStorageFile<TValue>
    {
        internal Enumerator<TParser, TValue> Implementation { get; }

        public DecoratingEnumerator(Enumerator<TParser, TValue> impl) : base(impl.Parser)
        {
            Implementation = impl;
        }

        public override void Dispose()
        {
            Implementation.Dispose();

            base.Dispose();
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

        public override void Skip(int skipCount) => Implementation.Skip(skipCount);
    
        public override TValue ElementAt(int index) => Implementation.ElementAt(index);
        public override TValue ElementAtOrDefault(int index) => Implementation.ElementAtOrDefault(index);

        public override TValue Last() => Implementation.Last();
    }
}
