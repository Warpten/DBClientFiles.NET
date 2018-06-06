using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DBClientFiles.NET.Analyzers
{
    internal class ClassDeclarationSyntaxComparer : IEqualityComparer<ClassDeclarationSyntax>
    {
        public static readonly ClassDeclarationSyntaxComparer Instance = new ClassDeclarationSyntaxComparer();

        public bool Equals(ClassDeclarationSyntax x, ClassDeclarationSyntax y)
        {
            if (x == null) return y == null;
            if (y == null) return false;

            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(ClassDeclarationSyntax obj)
        {
            return obj.GetHashCode();
        }
    }
}