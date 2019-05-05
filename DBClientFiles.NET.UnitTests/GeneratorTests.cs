using System;
using System.Collections.Generic;
using System.Text;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Parsing.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBClientFiles.NET.UnitTests
{
    [TestClass]
    public class GeneratorTests
    {
        public class SimpleType
        {
            [Cardinality(SizeConst = 2)]
            public Compound[] A;
            public int B;
            public float C;
        }

        public class Compound
        {
            public int X;
            public string Dummy;
        }

        [TestMethod]
        public void TestGenerator()
        {

        }
    }
}
