using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DBClientFiles.NET.Parsing.Reflection
{
    internal interface IMemberToken
    {
        TypeTokenType MemberType { get; }

        bool IsArray { get; }
        bool IsReadOnly { get; }

        TypeToken TypeToken { get; }
        TypeToken DeclaringTypeToken { get; }

        T GetAttribute<T>() where T : Attribute;
    }
}
