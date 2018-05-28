using System;

namespace DBClientFiles.NET.Attributes
{
    /// <summary>
    /// This attribute is used to specify to the library that any possible remainder of data in the relationship table
    /// is duplicate data and should be discarded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreRelationshipDataAttribute : Attribute
    {
    }
}
