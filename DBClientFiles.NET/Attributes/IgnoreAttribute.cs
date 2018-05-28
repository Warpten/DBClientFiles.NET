using System;

namespace DBClientFiles.NET.Attributes
{
    /// <summary>
    /// This attribute is used to decorate properties or fields that should not
    /// be (de)serialized (from) to the file.
    ///
    /// Not yet implemented.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreAttribute : Attribute
    {
    }
}
