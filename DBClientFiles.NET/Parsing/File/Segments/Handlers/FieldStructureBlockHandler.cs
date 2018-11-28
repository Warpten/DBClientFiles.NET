using DBClientFiles.NET.Internals.Binding;
using DBClientFiles.NET.Parsing.Binding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.File.Segments.Handlers
{
    internal class FieldStructureBlockHandler : IBlockHandler
    {
        public BlockIdentifier Identifier => BlockIdentifier.FieldInfo;

        public unsafe void ReadBlock<T>(T reader, long startOffset, long length) where T : BinaryReader, IParser
        {
            if (reader.Header.RecordCount == 0)
                return;

            var maxSize = 0u;
            var minSize = 0xFFFFFFFFu;

            BaseMemberMetadata previousNode = null;

            for (var i = 0; i < reader.Header.FieldCount; ++i)
            {
                var fieldInfo = reader.GetFileMemberMetadata(i);

                fieldInfo.Size = (uint)(4 - reader.ReadInt16() / 8);
                fieldInfo.Offset = reader.ReadUInt16();

                maxSize = Math.Max(fieldInfo.Size, maxSize);
                minSize = Math.Min(fieldInfo.Size, minSize);

                if (i >= 1)
                {
                    previousNode = reader.GetFileMemberMetadata(i - 1);
                    previousNode.Cardinality = (int)((fieldInfo.Offset - previousNode.Offset) / previousNode.Size);
                }
            }

            if (maxSize == minSize)
            {
                var lastNode = reader.GetFileMemberMetadata(reader.Header.FieldCount - 1);
                lastNode.Cardinality = (int)((reader.Header.RecordSize - lastNode.Offset) / lastNode.Size);
            }
        }

        public void WriteBlock<T, U>(T reader) where T : BinaryWriter, IWriter<U>
        {
            throw new NotImplementedException();
        }
    }
}
