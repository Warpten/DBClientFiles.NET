using Microsoft.VisualStudio.TestTools.UnitTesting;

using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Reflection;
using System.Linq.Expressions;
using System;

namespace DBClientFiles.NET.UnitTests
{
    [TestClass]
    public class IndexColumnTests
    {
        public class FlatType
        {
            public int    _1 { get; set; }
            public float  _2 { get; set; }
            public string _3 { get; set; }
            public int    _4 { get; set; }
        }

        public class NestedType
        {
            public FlatType _1 { get; set; }
            public float    _2 { get; set; }
            public string   _3 { get; set; }
            public int      _4 { get; set; }
        }

        public class SuperNestedType
        {
            public float      _1 { get; set; }
            public FlatType   _2 { get; set; }
            public NestedType _3 { get; set; }
            public int        _4 { get; set; }
            public string     _5 { get; set; }
            public int        _6 { get; set; }
        }

        [TestMethod, Description("Test - Index column - Flat")]
        public void TestFlatIndexLookup()
        {
            /*var typeInfo = new TypeInfo(typeof(FlatType));
            var serializer = new SerializerMock<FlatType>();
            serializer.Initialize(typeInfo, in StorageOptions.Default);
            serializer.SetIndexColumn(3);

            var instance = new FlatType {
                _1 = 1335,
                _2 = 42.0f,
                _3 = "choo choo",
                _4 = 1337 // Target
            };
            var extractedKey = serializer.GetKey(instance);
            Assert.AreEqual(1337, extractedKey);*/
        }

        [TestMethod, Description("Test - Index column - Nested once")]
        public void TestNestedOnceIndexLookup()
        {
            /*var typeInfo = new TypeInfo(typeof(NestedType));
            var serializer = new SerializerMock<NestedType>();
            serializer.Initialize(typeInfo, in StorageOptions.Default);
            serializer.SetIndexColumn(3);

            var instance = new NestedType
            {
                _1 = new FlatType
                {
                    _1 = 1335,
                    _2 = 42.0f,
                    _3 = "choo choo",
                    _4 = 1337 // Target
                },
                _2 = 3.14159265f,
                _3 = "motherfucker",
                _4 = 666 // Target 2
            };

            // Target 1
            var extractedKey = serializer.GetKey(instance);
            Assert.AreEqual(1337, extractedKey);

            // Recreate the serializer because once a delegate is created it's never relinquished
            serializer = new SerializerMock<NestedType>();
            serializer.Initialize(typeInfo, in StorageOptions.Default);
            serializer.SetIndexColumn(6);

            // Target 2
            extractedKey = serializer.GetKey(instance);
            Assert.AreEqual(666, extractedKey);*/
        }

        [TestMethod, Description("Test - Index column - Nested twice")]
        public void TestNestedTwiceIndexLookup()
        {
            /*var typeInfo = new TypeInfo(typeof(SuperNestedType));
            var serializer = new SerializerMock<SuperNestedType>();
            serializer.Initialize(typeInfo, in StorageOptions.Default);

            var instance = new SuperNestedType
            {
                _1 = 3.14159265f,
                _2 = new FlatType {
                    _1 = 21,
                    _2 = 2.2f,
                    _3 = "choo choo",
                    _4 = 24
                },
                _3 = new NestedType {
                    _1 = new FlatType
                    {
                        _1 = 311,
                        _2 = 31.2f,
                        _3 = "choo",
                        _4 = 314,
                    },
                    _2 = 3.2f,
                    _3 = "mother",
                    _4 = 34,
                },
                _4 = 4,
                _5 = "fucker",
                _6 = 6
            };

            // For some obscure reason the assertion doesn't trigger but the exception itself does.
            Assert.ThrowsException<InvalidOperationException>(() => serializer.SetIndexColumn(3)); // targets _2._3.

            serializer = new SerializerMock<SuperNestedType>();
            serializer.Initialize(typeInfo, StorageOptions.Default);
            serializer.SetIndexColumn(11); // targets _3._4.

            var extractedKey = serializer.GetKey(instance);
            Assert.AreEqual(34, extractedKey);*/
        }
    }

    internal sealed class SerializerMock<T> : StructuredSerializer<T>
    {
        // We aren't constructing an object anyways, so don't even bother.
        public override Expression VisitNode(Expression memberAccess, Member memberInfo, Expression recordReader)
        {
            throw new NotImplementedException();
        }
    }
}
