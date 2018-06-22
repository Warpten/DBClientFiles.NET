using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Internals.Binding
{
    internal abstract class ObjectMapper
    {
        public TypeMapper Type { get; }

        protected ObjectMapper(Type objectType)
        {
            Type = new TypeMapper(objectType);
        }

        internal abstract void Initialize(Stream fileStream);

        internal void Resolve()
        {

        }
    }
}
