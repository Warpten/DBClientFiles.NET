using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Exceptions
{
    public sealed class InvalidStructureException : Exception
    {
        public InvalidStructureException(string message) : base(message) { }
    }
}
