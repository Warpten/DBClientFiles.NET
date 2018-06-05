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
        public IReader<T> FileReader { get; private set; }

        public CodeGenerator<T> Generator { get; private set; }

        #region Life and death
        public void Dispose()
        {
            if (Options.CopyToMemory)
                Stream?.Dispose();
            Stream = null;

            Options = null;

            FileReader = null;
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

            _memberInfos = new ExtendedMemberInfoCollection(typeof(T), options);
        }
        #endregion

        private ExtendedMemberInfoCollection _memberInfos;

        public TKey ExtractKey<TKey>(T instance) where TKey : struct
        {
            return FileReader.ExtractKey<TKey>(instance);
        }

        public void InitializeReader(bool forced = false) => InitializeReader<int>(forced);

        public void InitializeReader<TKey>(bool forced = false)
            where TKey : struct
        {
            if (FileReader != null)
            {
                if (forced)
                    FileReader.Dispose();
                else
                    return;
            }

            Signature = (Signatures)(Stream.ReadByte() | (Stream.ReadByte() << 8) | (Stream.ReadByte() << 16) | (Stream.ReadByte() << 24));

            switch (Signature)
            {
                case Signatures.WDBC:
                    FileReader = new WDBC<TKey, T>(Stream, Options);
                    break;
                case Signatures.WDB2:
                    FileReader = new WDB2<TKey, T>(Stream, Options);
                    break;
                case Signatures.WDB5:
                    FileReader = new WDB5<TKey, T>(Stream, Options);
                    break;
                case Signatures.WDB6:
                    FileReader = new WDB6<TKey, T>(Stream, Options);
                    break;
                case Signatures.WDB3:
                case Signatures.WDB4:
                    throw new NotSupportedVersionException($"{Signature} files cannot be read without client metadata.");
                case Signatures.WDC1:
                    FileReader = new WDC1<TKey, T>(Stream, Options);
                    break;
                case Signatures.WDC2:
                    FileReader = new WDC2<TKey, T>(Stream, Options);
                    break;
                default:
                    throw new NotSupportedVersionException($"Unknown signature 0x{(int)Signature:X8}!");
            }
        }

        public void ReadHeader()
        {
            FileReader.MemberStore = _memberInfos;

            if (!FileReader.ReadHeader())
                throw new InvalidOperationException("Unable to read file header!");

            FileReader.MapRecords();
            FileReader.MemberStore.MapMembers();
        }

        public IEnumerable<T> Enumerate()
        {
            // Steal the generator
            Generator = FileReader.Generator;

            TableHash = FileReader.TableHash;
            LayoutHash = FileReader.LayoutHash;

            FileReader.ReadSegments();
            return FileReader.ReadRecords();
        }
    }
}
