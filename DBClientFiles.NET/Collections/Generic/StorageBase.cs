using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals;
using DBClientFiles.NET.Internals.Versions;
using System.IO;

namespace DBClientFiles.NET.Collections.Generic
{
    public interface IStorage
    {
        Signatures Signature { get; }
    }

    /// <summary>
    /// TODO: Turn this into an interface. Exposing this class is not good.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class StorageBase<TValue> : IStorage
        where TValue : class, new()
    {
        internal StorageOptions Options { get; set; }

        public Signatures Signature {
            get;
            private set;
        }
        private IReader<TValue> _fileReader;

        protected virtual void FromStream<TKey>(Stream fileStream, StorageOptions options) where TKey : struct
        {
            Options = options;

            Signature = (Signatures)((fileStream.ReadByte()) | (fileStream.ReadByte() << 8) | (fileStream.ReadByte() << 16) | (fileStream.ReadByte() << 24));

            switch (Signature)
            {
                case Signatures.WDBC:
                    _fileReader = new WDBC<TValue>(fileStream);
                    break;
                case Signatures.WDB2:
                    _fileReader = new WDB2<TValue>(fileStream);
                    break;
                case Signatures.WDB5:
                    _fileReader = new WDB5<TKey, TValue>(fileStream);
                    break;
                case Signatures.WDB6:
                    _fileReader = new WDB6<TKey, TValue>(fileStream);
                    break;
                case Signatures.WDB3:
                case Signatures.WDB4:
                    throw new NotSupportedVersionException($"{Signature} files cannot be read without client metadata.");
                case Signatures.WDC1:
                    _fileReader = new WDC1<TKey, TValue>(fileStream);
                    break;
                case Signatures.WDC2:
                    _fileReader = new WDC2<TKey, TValue>(fileStream);
                    break;
                default:
                    throw new NotSupportedVersionException($"Unknown signature 0x{(int)Signature:X8}!");
            }

            _fileReader.Options = options;
            if (!_fileReader.ReadHeader())
                return;

            _fileReader.ReadSegments();
        }

        protected virtual void FromStream(Stream fileStream, StorageOptions options)
        {
            FromStream<int>(fileStream, options);
        }

        internal abstract void LoadRecords(IReader<TValue> reader);

        protected void LoadRecords()
        {
            LoadRecords(_fileReader);
        }
    }
}
