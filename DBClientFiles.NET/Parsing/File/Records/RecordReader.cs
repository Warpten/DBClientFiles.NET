using System;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.File.Records
{
    /// <summary>
    /// This is the basic interface for reading primitives.
    /// </summary>
    public interface IRecordReader : IDisposable
    {
        T Read<T>() where T : unmanaged;
        T ReadImmediate<T>(int bitOffset, int bitCount) where T : unmanaged;
        
        string ReadString();
        string ReadString(int bitOffset, int bitCount);
    }

    internal static class _IRecordReader
    {
        public static readonly MethodInfo Read = typeof(IRecordReader).GetMethod("Read", Type.EmptyTypes);
        public static readonly MethodInfo ReadImmediate = typeof(IRecordReader).GetMethod("ReadImmediate", new[] { typeof(int), typeof(int) });

        // public static readonly MethodInfo ReadArray = typeof(IRecordReader).GetMethod("ReadArray", new[] { typeof(int) });
        // public static readonly MethodInfo ReadArrayPacked = typeof(IRecordReader).GetMethod("ReadArray", new[] { typeof(int), typeof(int) });

        public static readonly MethodInfo ReadString = typeof(IRecordReader).GetMethod("ReadString", Type.EmptyTypes);
        public static readonly MethodInfo ReadStringImmediate = typeof(IRecordReader).GetMethod("ReadString", new[] { typeof(int), typeof(int) });

        // public static readonly MethodInfo ReadStringArray = typeof(IRecordReader).GetMethod("ReadStringArray", new[] { typeof(int) });
        // public static readonly MethodInfo ReadStringArrayPacked = typeof(IRecordReader).GetMethod("ReadStringArray", new[] { typeof(int), typeof(int) });
    }
}
