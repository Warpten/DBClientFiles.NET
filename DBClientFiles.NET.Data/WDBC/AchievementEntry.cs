using DBClientFiles.NET.Attributes;

/// <summary>
/// Generated for 3.3.5.12340
/// </summary>
namespace DBClientFiles.NET.Data.WDBC
{
    [DBFileName(Name = "Achievement", Extension = FileExtension.DBC)]
    public sealed class AchievementEntry
    {
        [Index]
        public int ID { get; set; }
        public int Faction { get; set; }
        public int MapID { get; set; }
        public uint Supercedes { get; set; }
        [StoragePresence(StoragePresence.Include, SizeConst = 16)]
        public string[] Title { get; set; }
        public uint TitleFlags { get; set; }
        [StoragePresence(StoragePresence.Include, SizeConst = 16)]
        public string[] Description { get; set; }
        public uint Description_flags { get; set; }
        public uint Category { get; set; }
        public uint Points { get; set; }
        public uint UIOrder { get; set; }
        public uint Flags { get; set; }
        public uint IconID { get; set; }
        [StoragePresence(StoragePresence.Include, SizeConst = 16)]
        public string[] Rewards { get; set; }
        public uint RewardFlags { get; set; }
        public uint MinimumCriteria { get; set; }
        public uint SharesCriteria { get; set; }
    }

    public sealed class C3Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    [DBFileName(Name = "AreaTrigger", Extension = FileExtension.DBC)]
    public sealed class AreaTriggerEntry
    {
        public uint ID { get; set; }
        public uint MapID { get; set; }
        public C3Vector Position { get; set; }
        public float radius { get; set; }
        public float box_x { get; set; }
        public float box_y { get; set; }
        public float box_z { get; set; }
        public float box_orientation { get; set; }
    }

    [DBFileName(Name = "AuctionHouse", Extension = FileExtension.DBC)]
    public sealed class AuctionHouseEntry
    {
        public uint ID { get; set; }
        public uint FactionID { get; set; }
        public uint DepositPercent { get; set; }
        public uint CutPercent { get; set; }
        public string[] Name { get; set; }
        public uint NameFlags { get; set; }
    }

    [DBFileName(Name = "BankBagSlotPrices", Extension = FileExtension.DBC)]
    public sealed class BankBagSlotPricesEntry
    {
        public uint ID { get; set; }
        public uint Price { get; set; }
    }
}
