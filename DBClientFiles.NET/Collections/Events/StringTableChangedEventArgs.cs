using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Collections.Events
{
    public sealed class StringTableChangedEventArgs : EventArgs
    {
        public long Offset { get; }
        public string Value { get; }

        internal StringTableChangedEventArgs(long offset, string str)
        {
            Offset = offset;
            Value = str;
        }
    }
}
