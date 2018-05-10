using System;

namespace DBClientFiles.NET.Attributes
{
    public enum StoragePresence
    {
        Include,
        Exclude
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class StoragePresenceAttribute : Attribute
    {
        public StoragePresence Presence { get; set; }
        public int SizeConst { get; set; }

        public StoragePresenceAttribute(StoragePresence presence)
        {
            Presence = presence;
        }
    }
}
