using System;
namespace DBClientFiles.NET.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreRelationshipDataAttribute : Attribute
    {
    }
}
