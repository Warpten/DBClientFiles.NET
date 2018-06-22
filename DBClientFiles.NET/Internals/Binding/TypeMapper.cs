using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Internals.Binding
{
    public sealed class TypeMapper
    {
        public Type Type { get; }

        public IMemberMapper[] Members { get; internal set; }

        internal TypeMapper(Type objectType)
        {
            Type = objectType;
        }

        internal void Initialize(Stream fileStream)
        {

        }
    }
}
