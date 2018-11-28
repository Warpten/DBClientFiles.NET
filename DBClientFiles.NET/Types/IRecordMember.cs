namespace DBClientFiles.NET.Types
{
    /// <summary>
    /// An interface declaring the barebones basics of a <see cref="DynamicStructure"/>'s members.
    /// </summary>
    public interface IRecordMember
    {
        string Name { get; }

        int Cardinality { get; }

        RecordMemberType Type { get; }

        bool Signed { get; }
    }
}