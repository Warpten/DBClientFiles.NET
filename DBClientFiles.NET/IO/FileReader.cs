using DBClientFiles.NET.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using DBClientFiles.NET.Internals.Segments;

namespace DBClientFiles.NET.IO
{
    /// <summary>
    /// The basic class in charge of processing <code>.dbc</code> and <code>.db2</code> files.
    /// </summary>
    internal abstract class FileReader : BinaryReader
    {
        protected IFileHeader Header { get; }

        protected FileReader(IFileHeader header, Stream strm, bool keepOpen = false) : base(strm, Encoding.UTF8, keepOpen)
        {
            Header = header;
        }

        public override string ReadString()
        {
            var byteList = new List<byte>();
            byte currChar;
            while ((currChar = ReadByte()) != '\0')
                byteList.Add(currChar);

            return Encoding.UTF8.GetString(byteList.ToArray());
        }

        public abstract StorageOptions Options { get; }
        
        public abstract string FindStringByOffset(int tableOffset);
        
        protected abstract void ReleaseResources();

        protected override void Dispose(bool disposing)
        {
            ReleaseResources();
            base.Dispose(disposing);
        }
    }
}
