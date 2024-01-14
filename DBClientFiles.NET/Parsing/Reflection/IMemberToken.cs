using System;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal interface IMemberToken
    {
        TypeTokenKind MemberType { get; }

        bool IsArray { get; }
        bool IsReadOnly { get; }

        TypeToken TypeToken { get; }
        TypeToken DeclaringTypeToken { get; }

        T GetAttribute<T>() where T : Attribute;
    }
}
