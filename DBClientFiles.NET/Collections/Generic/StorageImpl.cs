using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals;
using DBClientFiles.NET.Internals.Versions;
using System;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Internals.Serializers;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Collections.Generic
{
    public interface IStorage
    {
        Signatures Signature { get; }
        uint TableHash { get; }
        uint LayoutHash { get; }
    }

    /// <summary>
    /// A basic implementation of IStorage that does all the heavy lifting. Used by DI in exposed containers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class StorageImpl<T> : IStorage, IDisposable
        where T : class, new()
    {
        #region IStorage
        public Signatures Signature { get; private set; }
        public uint TableHash { get; private set; }
        public uint LayoutHash { get; private set; }
        #endregion
        
        private StorageOptions Options { get; set; }
        private Stream Stream { get; set; }
        private IReader<T> _fileReader;

        public CodeGenerator<T> Generator { get; private set; }

        public void Dispose()
        {
            if (Options.CopyToMemory)
                Stream?.Dispose();
            Stream = null;

            Options = null;

            _fileReader = null;
        }

        public StorageImpl(Stream dataStream, StorageOptions options)
        {
            Options = options;

            if (options.CopyToMemory)
            {
                Stream = new MemoryStream((int)dataStream.Length);
                dataStream.CopyTo(Stream);
                Stream.Position = 0;
            }
            else
                Stream = dataStream;
        }

        public TKey ExtractKey<TKey>(T instance) where TKey : struct
        {
            return _fileReader.ExtractKey<TKey>(instance);
        }

        public IEnumerable<T> Enumerate() => Enumerate<int>();

        public void Initialize<TKey>()
            where TKey : struct
        {
            if (_fileReader != null)
                return;

            Signature = (Signatures)((Stream.ReadByte()) | (Stream.ReadByte() << 8) | (Stream.ReadByte() << 16) | (Stream.ReadByte() << 24));

            switch (Signature)
            {
                case Signatures.WDBC:
                    _fileReader = new WDBC<T>(Stream);
                    break;
                case Signatures.WDB2:
                    _fileReader = new WDB2<T>(Stream);
                    break;
                case Signatures.WDB5:
                    _fileReader = new WDB5<TKey, T>(Stream);
                    break;
                case Signatures.WDB6:
                    _fileReader = new WDB6<TKey, T>(Stream);
                    break;
                case Signatures.WDB3:
                case Signatures.WDB4:
                    throw new NotSupportedVersionException($"{Signature} files cannot be read without client metadata.");
                case Signatures.WDC1:
                    _fileReader = new WDC1<TKey, T>(Stream);
                    break;
                case Signatures.WDC2:
                    _fileReader = new WDC2<TKey, T>(Stream);
                    break;
                default:
                    throw new NotSupportedVersionException($"Unknown signature 0x{(int)Signature:X8}!");
            }

            _fileReader.Options = Options;
            
            if (!_fileReader.ReadHeader())
                throw new InvalidOperationException("Unable to read file header!");
        }

        public IEnumerable<T> Enumerate<TKey>()
            where TKey : struct
        {
            Initialize<TKey>();

            // Steal the generator
            Generator = _fileReader.Generator;

            TableHash = _fileReader.TableHash;
            LayoutHash = _fileReader.LayoutHash;

            _fileReader.ReadSegments();
            return _fileReader.ReadRecords();
        }
    }
}
