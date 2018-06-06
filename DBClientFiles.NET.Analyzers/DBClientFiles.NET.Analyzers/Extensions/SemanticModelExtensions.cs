using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DBClientFiles.NET.Analyzers.Extenstions
{
    public static class SemanticModelExtensions
    {
        public static IEnumerable<T> EnumerateNodes<T>(this SyntaxNode syntaxTree, int depth = -1)
            where T : SyntaxNode
        {
            foreach (var node in syntaxTree.ChildNodes())
            {
                if (depth > 0)
                    foreach (var subNode in node.EnumerateNodes<T>(depth - 1))
                        yield return subNode;

                if (depth == -1)
                    foreach (var subNode in node.EnumerateNodes<T>(-1))
                        yield return subNode;

                if (node is T tNode)
                    yield return tNode;
            }
        }
    }
}
