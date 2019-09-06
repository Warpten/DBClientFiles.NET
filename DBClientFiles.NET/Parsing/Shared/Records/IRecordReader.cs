using System;
using System.IO;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    /// <summary>
    /// This is the basic interface for reading primitives.
    /// </summary>
    public interface IRecordReader : IDisposable
    {
        T ReadImmediate<T>(int bitOffset, int bitCount) where T : unmanaged;
        
        string ReadString(int bitOffset, int bitCount);
    }

    public interface ISequentialRecordReader : IDisposable
    {
        T Read<T>(Stream inputStream) where T : unmanaged;
        string ReadString(Stream inputStream);
    }

    internal static class _IRecordReader
    {
        public static readonly MethodInfo ReadImmediate = typeof(IRecordReader).GetMethod("ReadImmediate", new[] { typeof(int), typeof(int) });

        public static readonly MethodInfo ReadStringImmediate = typeof(IRecordReader).GetMethod("ReadString", new[] { typeof(int), typeof(int) });
    }


    internal static class _ISequentialRecordReader
    {
        public static readonly MethodInfo Read = typeof(ISequentialRecordReader).GetMethod("Read", new[] { typeof(Stream) });
        public static readonly MethodInfo ReadString = typeof(ISequentialRecordReader).GetMethod("ReadString", new[] { typeof(Stream) });
    }
}
