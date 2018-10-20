using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using System;
using System.IO;

namespace DBClientFiles.NET.Runner
{
    public class C2Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class C3Vector
    {
        public C2Vector Position2D { get; set; }
        public float Z { get; set; }

        public float X => Position2D.X;
        public float Y => Position2D.Y;
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

    class Program
    {
        static void Main(String[] args)
        {
            using (var fs = File.OpenRead(@"D:\World of Warcraft 3.3.5\dbc\AreaTrigger.dbc"))
            {
                var collection = new StorageList<AreaTriggerEntry>(StorageOptions.Default, fs);
                Console.WriteLine(collection[0].MapID);
            }

            Console.ReadKey();
        }
    }
}
