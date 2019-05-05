using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DBClientFiles.NET.Runner
{
    //WDC1
    public sealed class AchievementEntry
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reward { get; set; }
        public int Flags { get; set; }
        public short InstanceID { get; set; }
        public short Supercedes { get; set; }
        public short Category { get; set; }
        public short UIOrder { get; set; }
        public short SharesCriteria { get; set; }
        public byte FactionID { get; set; }
        public byte Points { get; set; }
        public byte MinimumCriteria { get; set; }
        public int ID { get; set; }
        public int IconFileID { get; set; }
        public int CriteriaTreeID { get; set; }
    }

    //WDBC
    public sealed class CplxAchievement
    {
        public int ID { get; set; }
        public int FactionID { get; set; }
        public int MapId { get; set; }
        public int PreviousID { get; set; }
        public LocString Title { get; set; }
        public LocString Description { get; set; }
        public int Category { get; set; }
        public int POints { get; set; }
        public int UIOrder { get; set; }
        public int Flags { get; set; }
        public int IconID { get; set; }
        public LocString Reward { get; set; }
        public int MinimumCriteria { get; set; }
        public int LinkedAchievementID { get; set; }
    }

    public sealed class LocString
    {
        [Cardinality(SizeConst = 16)]
        public string[] Values { get; set; }
        public int Mask { get; set; }
    }

    class Program
    {
        static unsafe void Main(string[] args)
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\Achievement.dbc"))
            {
                var collection = new StorageEnumerable<CplxAchievement>(StorageOptions.Default, fs);
                InspectObject(collection.First());
            }

            Console.ReadKey();
        }

        private static void InspectObject(object obj)
        {
            PrintValue("model", obj);
        }

        private static void PrintValue(string prefix, object value)
        {
            if (value.GetType() == typeof(string) || value.GetType().IsPrimitive)
            {
                if (value.GetType() == typeof(string))
                    Console.WriteLine($@"{prefix.PadRight(50, ' ')}""{value}""");
                else if (value.GetType() == typeof(int) || value.GetType() == typeof(uint))
                    Console.WriteLine($"{prefix.PadRight(50, ' ')}{value} (0x{value:X8})");
                else
                    Console.WriteLine($"{prefix.PadRight(50, ' ')}{value}");
            }
            else
            {
                var props = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    var propValue = prop.GetValue(value);
                    if (propValue == null)
                    {
                        Console.WriteLine($"{prefix}.{prop.Name}] = null");

                        continue;
                    }

                    if (propValue.GetType().IsArray)
                    {
                        var valArray = (Array)propValue;
                        for (var i = 0; i < valArray.Length; ++i)
                            PrintValue($"{prefix}.{prop.Name}[{i}]", valArray.GetValue(i));
                    }
                    else
                    {
                        PrintValue($"{prefix}.{prop.Name}", propValue);
                    }
                }
            }
        }

    }
}
