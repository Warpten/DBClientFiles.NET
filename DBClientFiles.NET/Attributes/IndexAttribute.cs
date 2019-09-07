using System;

namespace DBClientFiles.NET.Attributes
{
    /// <summary>
    /// This attribute indicates which member is to be considered the index column. This attribute may be unnecessary for
    /// older file formats, where the index column is guaranteed to be first in the record. However, the index column may 
    /// be stored outside of record data for more recent file formats, or in the middle of it. For the most part, the
    /// library tries to guess which column is the index column, based off any available information. If you're not sure,
    /// just slap this attribute in - if you're wrong, the library will (rather loudly) complain. Only fields or properties
    /// of type <code>uint</code> or <code>int</code> may be decorated with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IndexAttribute : Attribute
    {
    }
}
