using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DBClientFiles.NET.Collections.Events;
using DBClientFiles.NET.IO;

namespace DBClientFiles.NET.Internals.Segments.Readers
{
    internal sealed class StringTableSegment : SegmentReader
    {
        public StringTableSegment(FileReader reader) : base(reader)
        {
            _cachedStringValues = new Dictionary<int, string>();
        }
        
        private readonly Dictionary<int, string> _cachedStringValues;

        public event EventHandler<StringTableChangedEventArgs> OnStringRead;

        public override void Read()
        {
            if (Segment.Length == 0)
                return;
            
            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);

            var stringSegment = FileReader.ReadBytes(Segment.Length);
            
            var stringStart = 2;
            for (var i = 3; i < Segment.Length;)
            {
                var isStringTermination = stringSegment[i] == 0;
                if (isStringTermination)
                {
                    var stringOffset = stringStart;
                    var stringLength = i - stringStart;

#if NETCOREAPP2_1
                    var stringValue = string.Create(stringLength, stringSegment,
                        (outSpan, inSegment) =>
                        {
                            for (var charIdx = 0; charIdx < outSpan.Length; ++charIdx)
                                outSpan[charIdx] = (char)inSegment[stringOffset + charIdx];
                        });
#else
                    var stringValue = System.Text.Encoding.UTF8.GetString(stringSegment, stringOffset, stringLength);
#endif
                    var correctedOffset = (int) (stringOffset + Segment.StartOffset);

                    _cachedStringValues[correctedOffset] = FileReader.Options.InternStrings ? string.Intern(stringValue) : stringValue;

                    OnStringRead?.Invoke(this, new StringTableChangedEventArgs(correctedOffset, stringValue));

                    stringStart = ++i;
                }
                else
                {
                    ++i;
                }
            }
        }

        protected override void Release()
        {
            _cachedStringValues.Clear();
        }

        public string this[int offset]
        {
            get
            {
                if (offset == 0)
                    return string.Empty;

                if (_cachedStringValues.TryGetValue(offset, out var cachedStringValue))
                    return cachedStringValue;

                return null;
            }
        }
    }
}
