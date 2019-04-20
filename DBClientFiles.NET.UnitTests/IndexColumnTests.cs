using Microsoft.VisualStudio.TestTools.UnitTesting;

using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Reflection;
using System.Linq.Expressions;
using System;
using DBClientFiles.NET.Parsing.File;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.UnitTests
{
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
        internal struct DummyHeader : IFileHeader
        {
            public int Size => throw new NotImplementedException();
            public Signatures Signature => throw new NotImplementedException();
            public uint TableHash => throw new NotImplementedException();
            public uint LayoutHash => throw new NotImplementedException();
            public int RecordSize => throw new NotImplementedException();
            public int RecordCount => throw new NotImplementedException();
            public int FieldCount => throw new NotImplementedException();
            public int StringTableLength => throw new NotImplementedException();
            public int MinIndex => throw new NotImplementedException();
            public int MaxIndex => throw new NotImplementedException();
            public int CopyTableLength => throw new NotImplementedException();

            public short IndexColumn { get; set; }

            public bool HasIndexTable => throw new NotImplementedException();
            public bool HasForeignIds => throw new NotImplementedException();
            public bool HasOffsetMap => throw new NotImplementedException();

            public DummyHeader(short indexColumn)
            {
                IndexColumn = indexColumn;
            }
        }

        public TypeToken Type { get; } = new TypeToken(typeof(T));

        private readonly IFileHeader _header;
        public ref readonly IFileHeader Header => ref _header;
        public ref readonly StorageOptions Options => ref StorageOptions.Default;

        public void Dispose()
        {
        }

        public U FindBlockHandler<U>(BlockIdentifier identifier) where U : IBlockHandler
        {
            throw new NotSupportedException();
        }

        public DummyFile(int indexColumn)
        {
            _header = new DummyHeader((short)indexColumn);
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
