using DBClientFiles.NET.Parsing.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.UnitTests
{
    [TestClass]
    public class TypeTraversalTest
    {
        public int DummyProperty { get; set; }
        public int[] DummyArrayProperty { get; set; }

        [TestMethod]
        public void TestType()
        {
            var typeInfo = new TypeToken(typeof(TypeTraversalTest));
        }
    }
}
