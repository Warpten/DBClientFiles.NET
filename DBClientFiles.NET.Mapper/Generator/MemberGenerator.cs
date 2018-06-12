using System;
using System.Reflection.Emit;

namespace DBClientFiles.NET.Mapper.Generator
{
    internal abstract class MemberGenerator
    {
        public TypeGenerator Parent { get; }
        public string Name { get; }
        public Type Type { get; set; }
        public int Cardinality { get; set; }
        public bool IsIndex { get; set; }
        public int Index { get; }

        internal MemberGenerator(TypeGenerator parent, string fieldName, Type fieldType, int index)
        {
            Parent = parent;
            Name = fieldName;
            Type = fieldType;
            Index = index;
        }

        public abstract void Generate(TypeBuilder builder);
    }
}