﻿using System;
using DBClientFiles.NET.Collections;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Utils;

namespace DBClientFiles.NET.IO
{
    /// <summary>
    /// Metainformation holder for <see cref="FileReader"/>.
    /// </summary>
    internal static class _FileReader
    {
        public static MethodInfo ReadString = typeof(FileReader).GetMethod("ReadString", new[] { typeof(int) });
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

        public abstract StorageOptions Options { get; set; }
        
        public abstract string FindStringByOffset(int tableOffset);
        
        protected abstract void ReleaseResources();

        protected override void Dispose(bool disposing)
        {
            ReleaseResources();
            base.Dispose(disposing);
        }
    }
}
