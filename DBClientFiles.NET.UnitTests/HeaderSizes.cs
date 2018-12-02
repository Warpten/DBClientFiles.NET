using DBClientFiles.NET.Parsing.File;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.UnitTests
{
    [TestClass]
    public class HeaderSizes
    {
        private static void RunTest<T>(int expectedSize) where T : IFileHeader
        {
            T instance = default;
            // Expected size includes the size of the signature (which is FourCC)
            Assert.AreEqual(expectedSize - 4, instance.Size);
        }

        [TestMethod]
        public void TestWDBC()
        {
            RunTest<Parsing.File.WDBC.Header>(5 * 4);
            RunTest<Parsing.File.WDB2.Header>(12 * 4);
            RunTest<Parsing.File.WDB5.Header>(10 * 4 + 2 * 2);
        }
    }
}
