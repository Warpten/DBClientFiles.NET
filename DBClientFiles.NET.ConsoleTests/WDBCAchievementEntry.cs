using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.ConsoleTests
{
    public sealed class WDBCAchievementEntry
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

    public sealed class WDB2ItemEntry
    {
        [Index]
        public uint ID { get; set; }
        public uint Class { get; set; }
        public uint SubClass { get; set; }
        public int SoundOverrideSubclass { get; set; }
        public int Material { get; set; }
        public uint DisplayId { get; set; }
        public uint InventoryType { get; set; }
        public uint Sheath { get; set; }
    }
}
