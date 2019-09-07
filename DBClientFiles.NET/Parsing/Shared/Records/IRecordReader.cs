using System;
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

    internal static class _IRecordReader
    {
        public static readonly MethodInfo ReadImmediate = typeof(IRecordReader).GetMethod("ReadImmediate", new[] { typeof(int), typeof(int) });

        public static readonly MethodInfo ReadStringImmediate = typeof(IRecordReader).GetMethod("ReadString", new[] { typeof(int), typeof(int) });
    }
}
