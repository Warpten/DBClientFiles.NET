using System;

namespace DBClientFiles.NET.Attributes
{
    /// <summary>
    /// This is a meta-attribute to indicate a member is to be considered the index column. While probably not necessary for
    /// older file formats, where the index column is guaranteed to be first in the record. However, it may not even be in
    /// the record for newer file formats, or in the middle of the record. If you are unsure, try it one way, the library
    /// will complain if you're wrong.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class IndexAttribute : Attribute
    {
    }
}
