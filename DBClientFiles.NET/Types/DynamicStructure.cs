using System.Collections.Generic;

namespace DBClientFiles.NET.Types
{
    /// <summary>
    /// This class is the entry-point of runtime structure declaration.
    /// </summary>
    public sealed class DynamicStructure
    {
        private IList<IRecordMember> _recordMembers;

        public DynamicStructure(params IRecordMember[] members)
        {
            _recordMembers = new List<IRecordMember>(members);
        }

        public DynamicStructure()
        {
            _recordMembers = new List<IRecordMember>();
        }

        public DynamicStructure With(IRecordMember member)
        {
            _recordMembers.Add(member);
            return this;
        }
    }
}