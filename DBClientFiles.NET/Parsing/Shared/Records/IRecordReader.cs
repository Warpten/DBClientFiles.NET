using System;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Shared.Records
{
    /// <summary>
    /// This is the basic interface for reading primitives.
    /// </summary>
    public interface IRecordReader : IDisposable
    {
        /// <summary>
        /// Reads an immediate value from the file.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="bitOffset">Offset, in bits, of the value within a record.</param>
        /// <param name="bitCount">Amount of bits used by the value within a record.</param>
        /// <returns></returns>
        T ReadImmediate<T>(int bitOffset, int bitCount) where T : unmanaged;
        
        /// <summary>
        /// Reads an immediate string from the file.
        /// </summary>
        /// <param name="bitOffset">Offset, in bits, of the value within a record.</param>
        /// <param name="bitCount">Amount of bits used by the value within a record.</param>
        /// <returns></returns>
        string ReadString(int bitOffset, int bitCount);

        /// <summary>
        /// Reads an UTF-8 string from the file.
        /// </summary>
        /// <param name="bitOffset"></param>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        ReadOnlyMemory<byte> ReadUTF8(int bitOffset, int bitCount);

        /// <summary>
        /// Reads a value from the pallet block of a file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bitOffset">Offset, in bits, of the value within a record.</param>
        /// <param name="bitCount">Amount of bits used by the value within a record.</param>
        /// <returns></returns>
        /// <remarks>This function begins by reading a 32-bits integer from the immediate data stream and using that as an index into the pallet block.</remarks>
        T ReadPallet<T>(int bitOffset, int bitCount) where T : struct;

        /// <summary>
        /// Reads a value from the pallet block of a file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bitOffset">Offset, in bits, of the value within a record.</param>
        /// <param name="bitCount">Amount of bits used by the value within the record.</param>
        /// <param name="cardinality">The amount of elements this array contains.</param>
        /// <returns></returns>
        /// <remarks>This function behaves similarly to <see cref="ReadPallet{T}(int, int)"/> but treats the pallet block as contiguous values for the array.</remarks>
        T[] ReadPalletArray<T>(int bitOffset, int bitCount, int cardinality) where T : struct;

        /// <summary>
        /// Reads a value from the common block of a file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rawDefaultValue">The default value's raw representation.</param>
        /// <returns></returns>
        T ReadCommon<T>(int rawDefaultValue) where T : struct;

        internal static class Methods
        {
            public static readonly MethodInfo ReadImmediate = typeof(IRecordReader).GetMethod("ReadImmediate", new[] { typeof(int), typeof(int) });
            public static readonly MethodInfo ReadStringImmediate = typeof(IRecordReader).GetMethod("ReadString", new[] { typeof(int), typeof(int) });
            public static readonly MethodInfo ReadUTF8Immediate = typeof(IRecordReader).GetMethod("ReadUTF8", new[] { typeof(int), typeof(int) });
            public static readonly MethodInfo ReadPallet = typeof(IRecordReader).GetMethod("ReadPallet", new[] { typeof(int), typeof(int) });
            public static readonly MethodInfo ReadPalletArray = typeof(IRecordReader).GetMethod("ReadPalletArray", new[] { typeof(int), typeof(int), typeof(int) });
            public static readonly MethodInfo ReadCommon = typeof(IRecordReader).GetMethod("ReadCommon", new[] { typeof(int), typeof(int), typeof(int) }); 
        }
    }
}
