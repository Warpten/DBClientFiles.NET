﻿using DBClientFiles.NET.Parsing.Shared.Segments.Handlers.Implementations;
using DBClientFiles.NET.Utils.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    /// <summary>
    /// A stream reader able to only read non-packed sequential values.
    /// </summary>
    internal readonly struct AlignedSequentialRecordReader
    {
        public static class Methods
        {
            public static readonly MethodInfo Read = typeof(AlignedSequentialRecordReader).GetMethod("Read", new[] { typeof(Stream) });
            public static readonly MethodInfo ReadString = typeof(AlignedSequentialRecordReader).GetMethod("ReadString", new[] { typeof(Stream) });
            public static readonly MethodInfo ReadUTF8 = typeof(AlignedSequentialRecordReader).GetMethod("ReadUTF8", new[] { typeof(Stream) });
        }

        private readonly StringBlockHandler _stringBlock;

        public AlignedSequentialRecordReader(StringBlockHandler stringBlock)
        {
            _stringBlock = stringBlock;
        }

        public void Dispose()
        {
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(Stream stream) where T : struct => stream.Read<T>();

        // Experiment....
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(Stream stream, ref T value) where T : struct => stream.Read(ref value);

        /// <summary>
        /// Reads an UTF-16 string from the buffer.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString(Stream stream) => _stringBlock.ReadString(stream.Read<int>());

        /// <summary>
        /// Reads a UTF-8 string from the buffer.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<byte> ReadUTF8(Stream stream) => _stringBlock.ReadUTF8(stream.Read<int>());
    }
}
