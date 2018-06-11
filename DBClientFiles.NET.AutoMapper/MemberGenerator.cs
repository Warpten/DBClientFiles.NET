using System;
using System.Reflection.Emit;

namespace DBClientFiles.NET.AutoMapper
{
    internal abstract class MemberGenerator
    {
        public TypeGenerator Parent { get; }
        public string Name { get; }
        public Type Type { get; set; }
        public int Cardinality { get; set; }
        public bool IsIndex { get; set; }
        public int Index { get; set; }

        internal MemberGenerator(TypeGenerator parent, string fieldName, Type fieldType)
        {
            Parent = parent;
            Name = fieldName;
            Type = fieldType;
        }

        public abstract void Generate(TypeBuilder builder);
    }
}