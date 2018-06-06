using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DBClientFiles.NET.Analyzers
{
    internal class StructureStore
    {
        private static readonly Lazy<StructureStore> _lazy = new Lazy<StructureStore>(() => new StructureStore());
        public static StructureStore Instance => _lazy.Value;

        private struct Node
        {
            public readonly ISymbol Property;
            public readonly ISymbol Field;
            public readonly TypeInfo Class;

            public Node(TypeInfo @class, ISymbol prop, ISymbol field)
            {
                Property = prop;
                Class = @class;
                Field = field;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Node n))
                    return false;

                return n.Property.Equals(Property) && n.Field.Equals(Field) && n.Class.GetHashCode() == Class.GetHashCode();
            }

            public override int GetHashCode()
            {
                var hashCode = 0;
                if (Property != null)
                    hashCode = Property.GetHashCode();
                if (Field != null)
                    hashCode ^= Field.GetHashCode();
                return hashCode ^ Class.GetHashCode();
            }
        }

        private HashSet<Node> _knownTypes = new HashSet<Node>();

        public void Cache(TypeInfo @class, ISymbol field, ISymbol property)
        {
            _knownTypes.Add(new Node(@class, property, field));
        }

        public ISymbol GetIndexProperty(ITypeSymbol symbol)
        {
            return _knownTypes.First(t => Equals(t.Class.Type, symbol)).Property;
        }

        public ISymbol GetIndexField(ITypeSymbol symbol)
        {
            return _knownTypes.First(t => Equals(t.Class.Type, symbol)).Field;
        }
    }
}
