using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.Internals.Segments
{
    internal sealed class CommonTable : Segment
    {
        // Size as specified in the header
        public int HeaderSize { get; set; }

        private MemoryStream Stream { get; set; }
        public int ColumnCount { get; private set; }

        private int[] EntryCounts { get; set; }
        private byte[] EntryTypes { get; set; }

        private Dictionary<int /* recordID */, long[] /* offset to field */> _offsetMap = new Dictionary<int, long[]>();

        public bool IsPacked { get; private set; } = false;

        public override void Read(BaseReader reader)
        {
            if (HeaderSize == 0)
                return;

            reader.BaseStream.Position = StartOffset;

            ColumnCount = reader.ReadInt32();

            EntryCounts = new int[ColumnCount];
            EntryTypes = new byte[ColumnCount];

            PreRead(reader, false);

            reader.BaseStream.Seek(4, SeekOrigin.Current);
            for (var i = 0; i < ColumnCount; ++i)
            {
                reader.BaseStream.Seek(5, SeekOrigin.Current);
                for (var j = 0; j < EntryCounts[i]; ++j)
                {
                    var recordID = reader.ReadInt32();
                    if (!_offsetMap.TryGetValue(recordID, out var offsetStore))
                    {
                        offsetStore = _offsetMap[recordID] = new long[ColumnCount];
                        Array.Clear(offsetStore, 0, ColumnCount);
                    }

                    offsetStore[i] = reader.BaseStream.Position;

                    switch (EntryTypes[j])
                    {
                        case 0: // String
                        case 3: // Float
                        case 4: // Integer
                            reader.BaseStream.Seek(4, SeekOrigin.Current);
                            break;
                        case 1: // Short
                            reader.BaseStream.Seek(IsPacked ? 2 : 4, SeekOrigin.Current);
                            break;
                        case 2: // Byte
                            reader.BaseStream.Seek(IsPacked ? 1 : 4, SeekOrigin.Current);
                            break;
                    }
                }
            }
        }

        private static MethodInfo ColumnReader = typeof(CommonTable).GetMethod("ReadColumn", new[] { typeof(BaseReader), typeof(int), typeof(int) });

        public Dictionary<TypeCode, MethodInfo> _methods = new Dictionary<TypeCode, MethodInfo>() {
            { TypeCode.UInt64, ColumnReader.MakeGenericMethod(typeof(ulong)) },
            { TypeCode.UInt32, ColumnReader.MakeGenericMethod(typeof(uint)) },
            { TypeCode.UInt16, ColumnReader.MakeGenericMethod(typeof(ushort)) },
            { TypeCode.Byte,   ColumnReader.MakeGenericMethod(typeof(byte)) },

            { TypeCode.Int64,  ColumnReader.MakeGenericMethod(typeof(long)) },
            { TypeCode.Int32,  ColumnReader.MakeGenericMethod(typeof(int)) },
            { TypeCode.Int16,  ColumnReader.MakeGenericMethod(typeof(short)) },
            { TypeCode.SByte,  ColumnReader.MakeGenericMethod(typeof(sbyte)) },

            { TypeCode.Single, ColumnReader.MakeGenericMethod(typeof(float)) },
        };

        public T ReadColumn<T>(BaseReader reader, int recordID, int columnIndex) where T : struct
        {
            var offset = _offsetMap[recordID][columnIndex];
            var oldPos = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            var value = reader.ReadStruct<T>();
            reader.BaseStream.Position = oldPos + (IsPacked ? SizeCache<T>.Size : 4);
            return value;
        }

        private void PreRead(BaseReader reader, bool packed)
        {
            var startOffset = reader.BaseStream.Position - 4;

            for (var i = 0; i < ColumnCount; ++i)
            {
                if (packed == false)
                {
                    EntryCounts[i] = reader.ReadInt32();
                    EntryTypes[i] = reader.ReadByte();
                }
                else
                    reader.BaseStream.Seek(5, SeekOrigin.Current);

                if (!packed)
                {
                    reader.BaseStream.Position += EntryCounts[i] * (4 + 4);
                }
                else
                {
                    switch (EntryTypes[i])
                    {
                        case 1: // Short
                            reader.BaseStream.Seek(2, SeekOrigin.Current);
                            break;
                        case 2: // Byte
                            reader.BaseStream.Seek(1, SeekOrigin.Current);
                            break;
                        case 0: // String
                        case 3: // Float
                        case 4: // Int
                            reader.BaseStream.Seek(4, SeekOrigin.Current);
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown entry type {(int)EntryTypes[i]}");
                    }
                }
            }

            var expectedSize = reader.BaseStream.Position - startOffset;

            reader.BaseStream.Position = startOffset + 4;

            if (expectedSize == HeaderSize)
                IsPacked = packed;
            else if (!packed) // todo this extra parse is not necessary but just for ensuring the code works we'll keep it until later (aka "as long as no one posts an issue about it")
                PreRead(reader, true);
            else
                throw new InvalidOperationException("F U C K I N G H E L L ");
        }
    }
}
