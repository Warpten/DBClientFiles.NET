using System;

namespace DBClientFiles.NET.Attributes
{
    public enum StoragePresence
    {
        Include,
        Exclude
    }

    /// <summary>
    /// This attribute really does not make much sense and should probably be removed. It is ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
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
