using System;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;
using DBClientFiles.NET.Test.Structures.WDBC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBClientFiles.NET.Test
{
    [TestClass]
    public class WDBC
    {
        [TestMethod]
        public void Achievement()
        {
            using (var fs = File.OpenRead(@"D:\Repositories\DBFilesClient.NET\Tests\WDBC\Files\Achievement.dbc"))
            {
                var reader = new StorageDictionary<int, AchievementEntry>(fs, StorageOptions.Default);
                Console.WriteLine(reader.Count);
            }
        }
    }
}
