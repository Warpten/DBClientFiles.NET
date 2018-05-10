using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Attributes
{
    /// <summary>
    /// This attribute is used to decorate properties or fields that should not
    /// be (de)serialized (from) to the file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreAttribute : Attribute
    {
    }
}
