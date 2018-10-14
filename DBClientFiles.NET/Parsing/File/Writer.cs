using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.File.Segments;
using DBClientFiles.NET.Parsing.Serialization;
using DBClientFiles.NET.Parsing.Types;

namespace DBClientFiles.NET.Parsing.File
{
    internal abstract class Writer<T> : IWriter<T>
    {
        protected Stream Stream { get; }
        private bool _keepOpen;

        public TypeInfo Type { get; }

        protected Block Head { get; set; }

        protected Writer(StorageOptions options, Stream outputStream, bool keepOpen)
        {
            Type = TypeInfo.Create<T>();

            Stream = outputStream;
            Options = options;

            Head = new Block();
            Head.Identifier = BlockIdentifier.Header;

            _keepOpen = keepOpen;
        }

        #region IWriter
        public StorageOptions Options { get; }

        public ISerializer<T> Serializer { get; set; }

        public void Dispose()
        {
            Block head = Head;
            while (head != null)
            {
                HandleBlock(head);
                head = head.Next;
            }

            if (!_keepOpen)
                Stream.Dispose();
        }
        #endregion

        public abstract void Insert(T instance);
        protected abstract void HandleBlock(Block block);
    }
}
