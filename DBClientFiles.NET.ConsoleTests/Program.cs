using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Test;
using System;
using System.IO;

namespace DBClientFiles.NET.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DBC Test");
            using (var achievementWDBC = File.OpenRead(@"D:\Repositories\DBFilesClient.NET\Tests\WDBC\Files\Achievement.dbc"))
            {
                var reader = new StorageList<WDBCAchievementEntry>(achievementWDBC);
                Console.WriteLine("{0} entries loaded.", reader.Count);
                Console.WriteLine();

                StructureTester.InspectInstance(reader[0]);
            }

            Console.WriteLine("DB2 Test");
            using (var achievementWDBC = File.OpenRead(@"D:\Repositories\DBFilesClient.NET\Tests\WDB2\Files\Item.db2"))
            {
                var reader = new StorageList<WDB2ItemEntry>(achievementWDBC);
                Console.WriteLine("{0} entries loaded.", reader.Count);
                Console.WriteLine();

                var structureTester = new StructureTester<WDB2ItemEntry>();
                StructureTester.InspectInstance(reader[0]);
            }

            Console.ReadKey();
        }
    }
}
