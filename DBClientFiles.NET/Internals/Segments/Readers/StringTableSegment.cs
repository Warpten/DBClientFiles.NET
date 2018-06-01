using System;
using System.Collections.Generic;
using System.IO;
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

        public event Action<int> OnStringRead;

        public override void Read()
        {
            if (Segment.Length == 0)
                return;
            
            FileReader.BaseStream.Seek(Segment.StartOffset, SeekOrigin.Begin);

            var stringSegment = FileReader.ReadBytes(Segment.Length);

            var stringEnd = Segment.Length - 1;
            for (var i = Segment.Length - 2; i >= 0;)
            {
                var isStringTermination = stringSegment[i] == 0;
                if (isStringTermination)
                {
                    var stringOffset = i + 1;
                    var stringLength = stringEnd - i;

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
                    _cachedStringValues[stringOffset] = FileReader.Options.InternStrings ? string.Intern(stringValue) : stringValue;
                    OnStringRead?.Invoke(stringOffset);

                    stringEnd = --i;
                }
                else
                {
                    --i;
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

                return string.Empty;
            }
        }
    }
}
