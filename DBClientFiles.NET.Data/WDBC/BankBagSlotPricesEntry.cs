using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDBC
{
    [DBFileName(Name = "BankBagSlotPrices", Extension = FileExtension.DBC)]
    public sealed class BankBagSlotPricesEntry
    {
        [Index]
        public uint ID { get; set; }
        public uint Price { get; set; }
    }
}