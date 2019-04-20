using System;
using System.Collections.Generic;

namespace DBClientFiles.NET.Utils
{
    public static class Linq
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> transform)
        {
            var stack = new Queue<T>();

            foreach (var node in source)
            {
                stack.Enqueue(node);
                foreach (var child in Flatten(transform(node), transform))
                    stack.Enqueue(child);

                while (stack.Count > 0)
                    yield return stack.Dequeue();
            }
        }
    }
}
