﻿using DBClientFiles.NET.Utils;
using System.IO;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.File.WDB2
{
    /// <summary>
    /// Representation of a WDB2 header.
    ///
    /// See <a href="http://www.wowdev.wiki/DBC">the wiki</a>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Header
    {
        public readonly Signatures Signature;
        public readonly int RecordCount;
        public readonly int FieldCount;
        public readonly int RecordSize;
        public readonly int StringTableLength;
        public readonly uint TableHash;
        public readonly uint Build;
        public readonly uint TimestampLastWritten;
        public readonly int MinIndex;
        public readonly int MaxIndex;
        public readonly int Locale;
        public readonly int CopyTableLength;

    }
}
