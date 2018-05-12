﻿using System;
using DBClientFiles.NET.Test.Structures.WDB2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBClientFiles.NET.Test
{
    [TestClass]
    public class WDB2
    {
        [TestMethod]
        public void Item()
        {
            var tester = new StorageTester<int, ItemEntry>();
            tester.TestListStorages(@"D:\Repositories\DBFilesClient.NET\Tests\WDB2\Files\Item.db2", 500);
            Console.WriteLine("File name                                                        Average                  Max                      Min");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0}{1}{2}{3}",
                "ItemEntry".PadRight(65),
                tester.AverageListTime.ToString().PadRight(25),
                tester.MaxListTime.ToString().PadRight(25), tester.MinListTime);
        }
    }
}