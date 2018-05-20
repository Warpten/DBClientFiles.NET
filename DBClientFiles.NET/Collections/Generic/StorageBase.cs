using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals;
using DBClientFiles.NET.Internals.Versions;
using System.IO;
using BinaryReader = DBClientFiles.NET.IO.BinaryReader;

namespace DBClientFiles.NET.Collections.Generic
{
    public abstract class StorageBase<TValue> where TValue : class, new()
    {
        internal StorageOptions Options { get; set; }
#if PERFORMANCE
        public System.TimeSpan LambdaGeneration { get; private set; }
#endif

        public Signatures Signature { get; private set; }

        protected virtual void FromStream<TKey>(Stream fileStream, StorageOptions options) where TKey : struct
        {
            Options = options;

            IReader<TValue> fileReader = null;
            Signatures signature;

            using (var reader = new BinaryReader(fileStream, true))
            {
                Signature = (Signatures)reader.ReadUInt32();
                switch (Signature)
                {
                    case Signatures.WDBC:
                        fileReader = new WDBC<TValue>(fileStream);
                        break;
                    case Signatures.WDB2:
                        fileReader = new WDB2<TValue>(fileStream);
                        break;
                    case Signatures.WDB5:
                        fileReader = new WDB5<TKey, TValue>(fileStream);
                        break;
                    case Signatures.WDB3:
                    case Signatures.WDB4:
                        throw new NotSupportedVersionException($"{Signature} files cannot be read without client metadata.");
                    default:
                        throw new NotSupportedVersionException($"Unknown signature 0x{(int)Signature:X8}!");
                }
            }

            fileReader.Options = options;
            if (fileReader == null || !fileReader.ReadHeader())
                return;

            fileReader.ReadSegments();
            LoadRecords(fileReader);

#if PERFORMANCE
            LambdaGeneration = fileReader.DeserializeGeneration;
#endif
        }

        protected virtual void FromStream(Stream fileStream, StorageOptions options)
        {
            FromStream<int>(fileStream, options);
        }

        internal abstract void LoadRecords(IReader<TValue> reader);
    }
}
