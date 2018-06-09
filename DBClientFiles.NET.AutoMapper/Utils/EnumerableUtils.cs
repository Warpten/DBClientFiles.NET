using System;
using System.Collections.Generic;
using System.Linq;

namespace DBClientFiles.NET.AutoMapper.Utils
{
    internal static class EnumerableUtils
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.DistinctBy(keySelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            return DistinctByImpl(source, keySelector, comparer);
        }

        private static IEnumerable<TSource> DistinctByImpl<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var knownKeys = new HashSet<TKey>(comparer);
            foreach (var element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TSource> UniqueBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.UniqueBy(keySelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<TSource> UniqueBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            return UniqueByImpl(source, keySelector, comparer);
        }

        private static IEnumerable<TSource> UniqueByImpl<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var knownKeys = new Dictionary<TKey, List<TSource>>(comparer);
            foreach (var element in source)
            {
                if (!knownKeys.TryGetValue(keySelector(element), out var inputList))
                    knownKeys[keySelector(element)] = inputList = new List<TSource>();

                inputList.Add(element);
            }

            return knownKeys.Where(kv => kv.Value.Count == 1).SelectMany(kv => kv.Value);
        }
    }
}
