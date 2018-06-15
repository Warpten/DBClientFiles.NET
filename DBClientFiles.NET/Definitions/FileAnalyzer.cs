using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Exceptions;
using DBClientFiles.NET.Internals;
using DBClientFiles.NET.Internals.Binding;
using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Versions.Headers;
using System;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Definitions
{
    /// <summary>
    /// If you need to load files, look in <see cref="Collections.Generic"/> instead.
    ///
    /// This object is a helper to be used when generating DBD definitions. It does not
    /// have any enumerating capabilities and will only read metadata informations from
    /// database files.
    /// </summary>
    public sealed class FileAnalyzer : IDisposable
    {
        public StorageOptions Options { get; }
        public Stream Stream { get; private set; }
        public Type RecordType { get; set; }

        private IReader File { get; set; }
        private IFileHeader Header { get; set; }
        public Signatures Signature => Header.Signature;
        public int IndexColumn => Header.IndexColumn;
        public uint LayoutHash => Header.LayoutHash;

        #region Life and death
        public void Dispose()
        {
            if (Options.CopyToMemory)
                Stream?.Dispose();

            Stream = null;
            File = null;
        }

        public FileAnalyzer(Stream dataStream, StorageOptions options) : this(null, dataStream, options)
        {
        }

        public FileAnalyzer(Type proposedType, Stream dataStream, StorageOptions options)
        {
            Options = options;
            RecordType = proposedType;

            if (options.CopyToMemory && (!dataStream.CanSeek || !(dataStream is MemoryStream)))
            {
                Stream = new MemoryStream((int)(dataStream.Length - dataStream.Position));

                dataStream.CopyTo(Stream);
                Stream.Position = 0;
            }
            else
                Stream = dataStream;

            Members = new ExtendedMemberInfoCollection(proposedType, options);
        }
        #endregion

        public ExtendedMemberInfoCollection Members { get; }

        public void Analyze()
        {
            InitializeHeaderInfo();
            InitializeFileReader();
            PrepareMemberInfo();
        }

        private void InitializeHeaderInfo()
        {
            Header = HeaderFactory.ReadHeader(Stream);

            Members.IndexColumn = Header.IndexColumn;
            Members.HasIndexTable = Header.HasIndexTable;
        }

        private void InitializeFileReader()
        {
            if (File != null)
                return;

            switch (Header.Signature)
            {
                case Signatures.WDBC:
                case Signatures.WDB2:
                case Signatures.WDB3:
                case Signatures.WDB4:
                    throw new NotSupportedException($"Unable to analyze {Header.Signature} files.");
                case Signatures.WDB5:
                {
                    Stream.Position = 48;
                    using (var binaryReader = new BinaryReader(Stream, Encoding.UTF8, true))
                        for (var i = 0; i < Header.FieldCount; ++i)
                            Members.AddFileMemberInfo(binaryReader);
                    break;
                }
                case Signatures.WDB6:
                {
                    Stream.Position = 56;
                    using (var binaryReader = new BinaryReader(Stream, Encoding.UTF8, true))
                        for (var i = 0; i < Header.FieldCount; ++i)
                            Members.AddFileMemberInfo(binaryReader);
                    break;
                }
                case Signatures.WDC1:
                {
                    using (var binaryReader = new BinaryReader(Stream, Encoding.UTF8, true))
                    {
                        Stream.Position = 48;
                        var totalFieldCount = binaryReader.ReadInt32();
                        Stream.Position = 68;
                        var totalFieldInfoSize = binaryReader.ReadInt32();
                        Stream.Position = 80;
                        var relationShipBlockSize = binaryReader.ReadInt32();
                        for (var i = 0; i < Header.FieldCount; ++i)
                            Members.AddFileMemberInfo(binaryReader);

                        Stream.Seek(-(totalFieldInfoSize + relationShipBlockSize), SeekOrigin.End);
                        for (var i = 0; i < totalFieldCount; ++i)
                            Members.FileMembers[i].ReadExtra(binaryReader);
                    }
                    break;
                }
                case Signatures.WDC2:
                {
                    using (var binaryReader = new BinaryReader(Stream, Encoding.UTF8, true))
                    {
                        Stream.Position = 44;
                        var totalFieldCount = binaryReader.ReadInt32();
                        Stream.Position = 68;
                        var sectionCount = binaryReader.ReadInt32();
                        Stream.Position += sectionCount * 9 * 4;

                        for (var i = 0; i < Header.FieldCount; ++i)
                            Members.AddFileMemberInfo(binaryReader);

                        for (var i = 0; i < totalFieldCount; ++i)
                            Members.FileMembers[i].ReadExtra(binaryReader);
                    }
                    break;
                }
                default:
                    throw new NotSupportedVersionException($"Unknown signature 0x{(int)Header.Signature:X8}!");
            }
        }

        private void PrepareMemberInfo()
        {
            if (Options.LoadMask.HasFlag(LoadMask.Records))
                Members.MapMembers();

            // Prepare arity of arrays and validate
            Members.CalculateCardinalities();
        }
    }
}
