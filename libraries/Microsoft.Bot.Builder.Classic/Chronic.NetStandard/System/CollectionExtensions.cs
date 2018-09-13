using System;
using System.Collections.Generic;

namespace Chronic
{
    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IList<T> collection, Action<T, T> action)
            where T : class
        {
            for (var i = 0; i < collection.Count; i++)
            {
                action(collection[i], (i < collection.Count - 1) ? collection[i + 1] : (T)null);
            }
        }

        public static void ForEach<T, TT>(this object[,] @this, Action<T, TT> action)
        {
            for (var i = 0; i < @this.GetLength(0); i++)
            {
                var pattern = (T)@this[i, 0];
                var replacement = (TT)Convert.ChangeType(@this[i, 1], typeof(TT));
                action(pattern, replacement);
            }
        }
    }
}
