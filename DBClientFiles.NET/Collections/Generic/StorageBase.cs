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

        protected virtual void FromStream<TKey>(Stream fileStream, StorageOptions options) where TKey : struct
        {
            Options = options;

            IReader<TValue> fileReader = null;
            Signatures signature;

            using (var reader = new BinaryReader(fileStream, true))
            {
                signature = (Signatures)reader.ReadUInt32();
                switch (signature)
                {
                    case Signatures.WDBC:
                        fileReader = new WDBC<TValue>(fileStream);
                        break;
                    case Signatures.WDB2:
                        fileReader = new WDB2<TValue>(fileStream);
                        break;
                    // case Signatures.WDB5:
                    //     fileReader = new WDB5<TKey>(fileStream);
                    //     break;
                    case Signatures.WDB3:
                    case Signatures.WDB4:
                        throw new NotSupportedVersionException($"{signature} files cannot be read without client metadata.");
                    default:
                        throw new NotSupportedVersionException($"Unknown signature 0x{(int)signature:X8}!");
                }
            }

            fileReader.Options = options;
            if (fileReader == null || !fileReader.ReadHeader())
                return;

            fileReader.ReadSegments();
            LoadRecords(fileReader);
        }

        protected virtual void FromStream(Stream fileStream, StorageOptions options)
        {
            FromStream<int>(fileStream, options);
        }

        internal abstract void LoadRecords(IReader<TValue> reader);
    }
}
