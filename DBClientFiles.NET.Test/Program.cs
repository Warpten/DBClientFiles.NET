using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DBClientFiles.NET.Runner
{
    public class C2Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class C3Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public sealed class LocString
    {
        [Cardinality(SizeConst = 16)]
        public string[] Values { get; set; }
        public uint Mask { get; set; }
    }

    public struct AreaTriggerEntry
    {
        public uint ID { get; set; }
        public uint MapID { get; set; }
        public C3Vector Position { get; set; }
        public float Radius { get; set; }
        public C3Vector BoxSize { get; set; }
        public float BoxOrientation { get; set; }
    }

    public sealed class AchievementEntry
    {
        public uint ID { get; set; }
        public int FactionID { get; set; }
        public int MapID { get; set; }
        public int ParentAchievementID { get; set; }
        public LocString Name { get; set; }
        public LocString Description { get; set; }
        public uint CategoryID { get; set; }
        public uint Points { get; set; }
        public uint UIOrder { get; set; }
        public uint Flags { get; set; }
        public uint IconID { get; set; }
        public LocString Rewards { get; set; }
        public uint MinimumCriteriaID { get; set; }
        public uint SharesCriteria { get; set; }
    }

    class Program
    {
        static unsafe void Main(String[] args)
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\Achievement.dbc"))
            {
                var collection = new StorageEnumerable<AchievementEntry>(StorageOptions.Default, fs);
                InspectObject(collection.First());
            }

            Console.ReadKey();
        }

        private static void InspectObject(object obj)
        {
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var value = prop.GetValue(obj);
                if (value == null)
                {
                    Console.WriteLine($"{prop.Name}] = null");

                    continue;
                }

                if (value.GetType().IsArray)
                {
                    var valArray = (Array) value;
                    for (var i = 0; i < valArray.Length; ++i)
                    {
                        Console.WriteLine($"{prop.Name}[{i}] = {valArray.GetValue(i)}");
                    }
                }
                else
                {
                    Console.WriteLine($"{prop.Name} = {value}");
                }
            }
        }

    }
}
