using System;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.File
{
    /// <summary>
    /// This is the basic interface for reading primitives.
    /// </summary>
    public interface IRecordReader : IDisposable
    {
        T Read<T>() where T : unmanaged;
        T Read<T>(int bitCount) where T : unmanaged;

        T[] ReadArray<T>(int count) where T : unmanaged;
        T[] ReadArray<T>(int count, int elementBitCount) where T : unmanaged;

        string ReadString();
        string ReadString(int bitCount);

        string[] ReadStringArray(int count);
        string[] ReadStringArray(int count, int elementBitCount);
    }

    internal static class _IRecordReader
    {
        public static readonly MethodInfo Read = typeof(IRecordReader).GetMethod("Read", Type.EmptyTypes);
        public static readonly MethodInfo ReadPacked = typeof(IRecordReader).GetMethod("Read", new[] { typeof(int) });

        public static readonly MethodInfo ReadArray = typeof(IRecordReader).GetMethod("ReadArray", new[] { typeof(int) });
        public static readonly MethodInfo ReadArrayPacked = typeof(IRecordReader).GetMethod("ReadArray", new[] { typeof(int), typeof(int) });

        public static readonly MethodInfo ReadString = typeof(IRecordReader).GetMethod("ReadString", Type.EmptyTypes);
        public static readonly MethodInfo ReadStringPacked = typeof(IRecordReader).GetMethod("ReadString", new[] { typeof(int) });

        public static readonly MethodInfo ReadStringArray = typeof(IRecordReader).GetMethod("ReadStringArray", new[] { typeof(int) });
        public static readonly MethodInfo ReadStringArrayPacked = typeof(IRecordReader).GetMethod("ReadStringArray", new[] { typeof(int), typeof(int) });
    }
}
