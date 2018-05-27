using DBClientFiles.NET.Internals.Versions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DBClientFiles.NET.IO
{
    internal static class _FileReader
    {
        public static MethodInfo ReadString { get; } = typeof(FileReader).GetMethod("ReadString", new[] { typeof(int) });
    }

    /// <summary>
    /// The basic class in charge of processing <code>.dbc</code> and <code>.db2</code> files.
    /// </summary>
    internal abstract class FileReader : BinaryReader
    {
        public FileReader(Stream strm, bool keepOpen = false) : base(strm, Encoding.UTF8, keepOpen)
        {
        }

        public override string ReadString()
        {
            var byteList = new List<byte>();
            byte currChar;
            while ((currChar = ReadByte()) != '\0')
                byteList.Add(currChar);

            return Encoding.UTF8.GetString(byteList.ToArray());
        }

        public abstract string ReadString(int tableOffset);
    }
}
