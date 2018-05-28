using DBClientFiles.NET.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Attributes
{
    /// <summary>
    /// This attribute is used to indicate the size of an array field or property.
    /// For older file formats (WDBC, WDB2 and WDB5 to some extent), it is non-trivial (or borderline impossible)
    /// to guess the size of arrays from the file itself alone. As such, this allows
    /// <see cref="StorageBase{TValue}"/> to continue execution where it would otherwise fail.
    ///
    /// Not that when serializing a record, if this attribute is absent from an array, the member's
    /// actual size will be used instead - resulting in possible situations where you would expect your structure to
    /// serialize to, say, <pre>int[5]</pre> while it would serialize to <pre>int[3]</pre> because that's just how many
    /// elements you allocated.
    /// </summary>
    /// <remarks>
    /// This attribute is designed as a replacement for <see cref="MarshalAsAttribute"/>, which has the side-effect
    /// of causing the library to deserialize simple types using the marshaller instead of fast pointer copies.
    /// 
    /// <see cref="MarshalAsAttribute"/> being handled is a backwards-compatibility feature.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class CardinalityAttribute : Attribute
    {
        public int SizeConst { get; set; }
    }
}
