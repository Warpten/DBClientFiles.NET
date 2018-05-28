using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDBC
{
    [DBFileName(Name = "AuctionHouse", Extension = FileExtension.DBC)]
    public sealed class AuctionHouseEntry
    {
        [Index]
        public uint ID { get; set; }
        public uint FactionID { get; set; }
        public uint DepositPercent { get; set; }
        public uint CutPercent { get; set; }
        public string[] Name { get; set; }
        public uint NameFlags { get; set; }
    }
}