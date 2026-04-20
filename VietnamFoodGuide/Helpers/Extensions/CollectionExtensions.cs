using System;
using System.Collections.Generic;
using System.Linq;

namespace VietnamFoodGuide.Helpers.Extensions
{
    /// <summary>
    /// Extension methods for collection operations
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Safe access to collection - returns empty list if null
        /// </summary>
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Check if collection is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Batch process items with callback
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null) return;
            foreach (var item in collection)
                action?.Invoke(item);
        }

        /// <summary>
        /// Batch process with index
        /// </summary>
        public static void ForEachWithIndex<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            if (collection == null) return;
            int index = 0;
            foreach (var item in collection)
            {
                action?.Invoke(item, index);
                index++;
            }
        }

        /// <summary>
        /// Distinct by predicate
        /// </summary>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> keySelector)
        {
            var seen = new HashSet<TKey>();
            return collection.Where(item => seen.Add(keySelector(item)));
        }
    }
}
