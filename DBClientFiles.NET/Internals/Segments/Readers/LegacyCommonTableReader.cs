using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Internals.Versions;
using DBClientFiles.NET.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    /// <summary>
    /// A segment reader for legacy common table (as seen in WDB6 file format).
    /// </summary>
    /// <typeparam name="TValue">The record type of the currently operated file.</typeparam>
    internal sealed class LegacyCommonTableReader<TKey, TValue> : ISegmentReader<TValue>
        where TValue : class, new()
    {
        public Segment<TValue> Segment { get; set; }
        public BaseFileReader<TValue> Storage => Segment.Storage;

        private bool _isPadded = true;
        private Type[] _memberTypes;

        private Dictionary<TKey, long>[] _valueOffsets;
        
        public LegacyCommonTableReader()
        {
            if (Segment.Length == 0)
                return;

            Storage.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);
            Parse();
        }

        private void Parse()
        {
            var columnCount = Storage.ReadInt32();

            _memberTypes = new Type[columnCount];
            _valueOffsets = new Dictionary<TKey, long>[columnCount];
            for (var i = 0; i < columnCount; ++i)
                _valueOffsets[i] = new Dictionary<TKey, long>();

            /// Try to read everything as unpacked.
            /// If reading fails (probably because the cursor is now beyond the end of the file), then the structure is packed.

            bool isDryRun = true;
            bool successfulParse = true;
            for (var i = 0; i < columnCount && successfulParse; ++i)
                successfulParse |= AssertReadColumn(i, isDryRun, false);

            _isPadded = successfulParse;
            if (successfulParse)
            {
                // Structure is actually packed.
                Storage.BaseStream.Seek(Segment.StartOffset + 4, SeekOrigin.Begin);

                for (var i = 0; i < columnCount && successfulParse; ++i)
                    successfulParse |= AssertReadColumn(i, false, true);
            }
            else
            {
                // Structure is unpacked.
                // Read again, but it's not a dry run this time.
                Storage.BaseStream.Seek(Segment.StartOffset + 4, SeekOrigin.Begin);

                for (var i = 0; i < columnCount; ++i)
                    AssertReadColumn(i, true, false);
            }

            // We also need to check here that we properly went through the entirety of the block - for safekeeping.
            if (Storage.BaseStream.Position != Segment.EndOffset)
                throw new FileLoadException();
        }

        /// <summary>
        /// Reads a column, also asserting if the common block is packed or not.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="dryRun"></param>
        /// <param name="readAsPadded"></param>
        /// <returns>true if the block read correctly, false otherwise.</returns>
        private bool AssertReadColumn(int columnIndex, bool dryRun, bool readAsPadded)
        {
            var entryCount = Storage.ReadInt32();
            var entryType = Storage.ReadByte();

            var dataSize = 4;
            switch (entryType)
            {
                case 0: // string
                    _memberTypes[columnIndex] = typeof(string);
                    break;
                case 3: // float
                    _memberTypes[columnIndex] = typeof(float);
                    break;
                case 4: // int
                    _memberTypes[columnIndex] = typeof(int);
                    break;
                case 1: // short
                    _memberTypes[columnIndex] = typeof(short);
                    if (!readAsPadded)
                        dataSize = 2;
                    break;
                case 2: // byte
                    _memberTypes[columnIndex] = typeof(byte);
                    if (!readAsPadded)
                        dataSize = 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (dryRun)
            {
                var newPosition = Storage.BaseStream.Seek((4 + dataSize) * entryCount, SeekOrigin.Current);
                // Hopefully when this happens it means we were trying to read as non-packed.
                if (newPosition > Segment.EndOffset)
                    return false;
                return true;
            }
            else
            {
                var keySize = Math.Min(4, SizeCache<TKey>.Size);

                for (var i = 0; i < entryCount; ++i)
                {
                    _valueOffsets[columnIndex][Storage.ReadStruct<TKey>()] = Storage.BaseStream.Position + keySize;
                    var newPosition = Storage.BaseStream.Seek(keySize + dataSize, SeekOrigin.Current);

                    // And same as the comment above here.
                    if (newPosition > Segment.EndOffset)
                        return false;
                }
                return true;
            }
        }

        public void Dispose()
        {
            Segment = null;
        }

        public void Read()
        {
            
        }

        public T ReadStructValue<T>(int columnIndex, TKey recordKey)
        {
            var dict = _valueOffsets[columnIndex];
            if (!dict.TryGetValue(recordKey, out var offset))
                return default;

            var previousOffset = Storage.BaseStream.Position;
            Storage.BaseStream.Position = offset;
            var value = Storage.ReadStruct<T>();
            Storage.BaseStream.Position = previousOffset;
            return value;
        }

        internal class CommonTableDeserializer
        {
            private LegacyCommonTableReader<TKey, TValue> _reader;

            private static Dictionary<Type, ConstructorInfo> _constructorCache = new Dictionary<Type, ConstructorInfo>();

            private static ConstructorInfo GetConstructor<T>() => GetConstructor(typeof(T));
            private static ConstructorInfo GetConstructor(Type type)
            {
                if (_constructorCache.TryGetValue(type, out var ctor))
                    return ctor;

                return _constructorCache[type] = type.GetConstructor(new[] { typeof(BinaryReader) });
            }

            public CommonTableDeserializer(LegacyCommonTableReader<TKey, TValue> reader)
            {
                _reader = reader;
            }

            private Func<StorageBase<TValue>, T> GenerateColumnReader<T>(int columnIndex, int deserializedSize)
            {
                if (deserializedSize <= 0 || deserializedSize > 4)
                    throw new ArgumentOutOfRangeException();

                var argExpr = Expression.Parameter(typeof(StorageBase<TValue>));
                var body = new List<Expression>();

                var oldPositionLocalVar = Expression.Variable(typeof(long));

                var streamPosExpr = Expression.MakeMemberAccess(argExpr, typeof(StorageBase<TValue>).GetProperty("BaseStream"));
                streamPosExpr = Expression.MakeMemberAccess(streamPosExpr, typeof(Stream).GetProperty("Position"));

                var returnValue = Expression.Variable(typeof(T));

                var valueCtor = GetConstructor<T>();
                if (valueCtor != null)
                {
                    if (typeof(T).IsValueType && _reader._isPadded)
                        body.Add(Expression.Assign(oldPositionLocalVar, streamPosExpr));

                    body.Add(Expression.Assign(returnValue, Expression.New(valueCtor, argExpr)));

                    if (typeof(T).IsValueType && _reader._isPadded)
                        body.Add(Expression.Assign(streamPosExpr, Expression.Add(oldPositionLocalVar, Expression.Constant(deserializedSize))));
                }
                else if (!typeof(T).IsValueType) // Not a value type, and missing ctor!
                {
                    throw new InvalidOperationException();
                }
                else // Value type
                {
                }

                body.Add(returnValue);

                var bodyExpr = Expression.Block(new[] { oldPositionLocalVar, returnValue }, body);
                var lambda = Expression.Lambda<Func<StorageBase<TValue>, T>>(bodyExpr, new[] { argExpr });
                var compiledMethod = lambda.Compile();
                return compiledMethod;
            }
        }
    }
}
