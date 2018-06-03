using System;
using System.IO;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Attributes
{
    /// <summary>
    /// This attribute indicates which member is to be considered the index column. While probably not necessary for
    /// older file formats, where the index column is guaranteed to be first in the record. However, it may not even be in
    /// the record for newer file formats, or in the middle of the record. If you are unsure, try it one way, the library
    /// will complain if you're wrong.
    /// </summary>
    /// <remarks>This attribute is not to be confused with <see cref="StorageDictionary{TKey,TValue}"/>'s lambda argument,
    /// which is instead used to define a member to key with in the storage.</remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class IndexAttribute : Attribute
    {
    }
}
