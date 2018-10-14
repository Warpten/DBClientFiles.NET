using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.File
{
    internal sealed class Proxy<T>
    {
        public T Instance { get; set; }
        public uint UUID { get; set; }
    }
}
