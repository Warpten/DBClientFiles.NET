using System;

namespace DBClientFiles.NET.Definitions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class LayoutAttribute : Attribute
    {
        public uint LayoutHash { get; set; }

        public LayoutAttribute(uint layoutHash)
        {
            LayoutHash = layoutHash;
        }
    }
}