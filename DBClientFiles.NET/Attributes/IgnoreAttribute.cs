using System;

namespace DBClientFiles.NET.Attributes
{
    /// <summary>
    /// This attribute is used to decorate properties or fields that should not
    /// be (de)serialized (from).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class IgnoreAttribute : Attribute
    {
    }
}
