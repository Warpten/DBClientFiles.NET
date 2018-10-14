using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.File.WDBC
{
    internal sealed class Writer<T> : File.Writer<T>
    {
        private Block _recordsBlock;
        private Block _stringBlock;

        public Writer(StorageOptions options, Stream outputStream, bool keepOpen) : base(options, outputStream, keepOpen)
        {
            Debug.Assert(options.MemberType == MemberTypes.Field || options.MemberType == MemberTypes.Property);

            // TODO: allocate serializer

            Head.Length = 20; // SizeCache<Header>.Size should work but let's be careful

            Head.Next = _recordsBlock = new Block()
            {
                Length = 0,
                Identifier = BlockIdentifier.Records,
            };

            Head.Next.Next = _stringBlock = new Block() {
                Length = 0,
                Identifier = BlockIdentifier.StringBlock
            };
        }

        public override void Insert(T instance)
        {
            _recordsBlock.Length += 0; // ?
            // foreach (var @string in Serializer.ExtractStrings(instance)) {
            //     _stringBlock.Length += @string.Length + 1;
            //     ???? Need to save them here too
            // }

            // Add to List<T> cache.
        }

        /// <summary>
        /// Handles writing the given block.
        /// </summary>
        /// <param name="block"></param>
        protected override void HandleBlock(Block block)
        {
            using (var writer = new BinaryWriter(Stream, Encoding.UTF8, true))
            {
                switch (block.Identifier)
                {
                    case BlockIdentifier.Header:
                        writer.Write((uint) Signatures.WDBC);
                        writer.Write(0); // Record Count
                        writer.Write(Options.MemberType == MemberTypes.Field
                            ? Type.Fields.Count()
                            : Type.Properties.Count());
                        writer.Write(0); // Record_size
                        writer.Write(_stringBlock.Length);
                        break;
                    case BlockIdentifier.Records:
                        break;
                    case BlockIdentifier.StringBlock:
                        break;
                }
            }
        }
    }
}
