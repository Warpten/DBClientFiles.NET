using Microsoft.VisualStudio.TestTools.UnitTesting;

using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Reflection;
using System.Linq.Expressions;
using System;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Parsing.File.Segments.Handlers.Implementations;
using System.IO;

namespace DBClientFiles.NET.UnitTests
{
#pragma warning disable IDE1006 // Naming Styles

    [TestClass]
    public class IndexColumnTests
    {
        public class FlatType
        {
            public int _1 { get; set; }
            public float _2 { get; set; }
            public string _3 { get; set; }
            public int _4 { get; set; }
        }

        public class NestedType
        {
            public FlatType _1 { get; set; }
            public float _2 { get; set; }
            public string _3 { get; set; }
            public int _4 { get; set; }
        }

        public class SuperNestedType
        {
            public float _1 { get; set; }
            [Cardinality(SizeConst = 2)]
            public FlatType[] _2 { get; set; }
            public NestedType _3 { get; set; }
            public int _4 { get; set; }
            public string _5 { get; set; }
            public int _6 { get; set; }
        }

        [TestMethod, Description("Test - Index column - Wrong type")]
        public void TestWrongIndexType()
        {
            var typeToken = new TypeToken(typeof(SuperNestedType));
            Expression root = Expression.Variable(typeof(SuperNestedType));
            var idx = 9;
            var member = typeToken.GetMemberByIndex(ref idx, ref root, TypeTokenType.Property);

            var dummyFile = new DummyFile<SuperNestedType>(3);

            var instance = new SuperNestedType
            {
                _1 = 3.14159265f,
                _2 = new FlatType[] {
                    new FlatType {
                        _1 = 210,
                        _2 = 2.20f,
                        _3 = "choo choo",
                        _4 = 240
                    },
                    new FlatType {
                        _1 = 211,
                        _2 = 2.21f,
                        _3 = "choo choo choo",
                        _4 = 241
                    }
                },
                _3 = new NestedType
                {
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

            var serializer = new SerializerMock<SuperNestedType>();
            var assertion = Assert.ThrowsException<InvalidOperationException>(() => serializer.Initialize(dummyFile));
            Assert.IsTrue(assertion.Message.StartsWith("Invalid structure:"));
        }

        [TestMethod, Description("Test - Index column - Nested")]
        public void TestNestedOnceIndexLookup()
        {
            var dummyFile = new DummyFile<SuperNestedType>(8);

            var instance = new SuperNestedType
            {
                _1 = 3.14159265f,
                _2 = new FlatType[] {
                    new FlatType {
                        _1 = 210,
                        _2 = 2.20f,
                        _3 = "choo choo",
                        _4 = 240
                    },
                    new FlatType {
                        _1 = 211,
                        _2 = 2.21f,
                        _3 = "choo choo choo",
                        _4 = 241
                    }
                },
                _3 = new NestedType
                {
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

            var serializer = new SerializerMock<SuperNestedType>();
            serializer.Initialize(dummyFile);

            var extractedKey = serializer.GetKey(instance);
            Assert.AreEqual(240, extractedKey);
        }
    }

    internal sealed class DummyFile<T> : IBinaryStorageFile
    {
        public TypeToken Type { get; } = new TypeToken(typeof(T));

        public ref readonly StorageOptions Options => ref StorageOptions.Default;

        public IHeaderHandler Header => throw new NotImplementedException();

        public int RecordCount => throw new NotImplementedException();

        public Stream BaseStream => throw new NotImplementedException();

        public void Dispose()
        {
        }

        public Block FindBlock(BlockIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public DummyFile(int indexColumn)
        {
        }
    }

    internal sealed class SerializerMock<T> : StructuredSerializer<T>
    {
        // We aren't constructing an object anyways, so don't even bother.
        public override Expression VisitNode(Expression memberAccess, MemberToken memberInfo, Expression recordReader)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore IDE1006 // Naming Styles
