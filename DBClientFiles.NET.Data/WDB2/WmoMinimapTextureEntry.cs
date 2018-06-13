using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Data.WDB2
{
    [DBFileName(Name = "WMOMinimapTexture.WDB2", Extension = FileExtension.DB2)]
    public sealed class WMOMinimapTextureEntry_725_24393
    {
        [Index]
        public int Id { get; set; }
        public int FileDataID { get; set; }
        public ushort WMOID { get; set; }
        public ushort GroupNum { get; set; }
        public byte BlockX { get; set; }
        public byte BlockY { get; set; }
    }
}
