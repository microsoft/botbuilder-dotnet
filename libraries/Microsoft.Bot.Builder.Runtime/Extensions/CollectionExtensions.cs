// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Runtime.Extensions
{
    /// <summary>
    /// Defines extension methods for <see cref="ICollection{T}"/>.
    /// </summary>
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Adds the specified set of items to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection's items.</typeparam>
        /// <param name="collection">The collection to be added to.</param>
        /// <param name="items">The items to be added to the collection.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            // If the collection is a standard type with a faster AddRange implementation, use it.
            switch (collection)
            {
                case List<T> list:
                    list.AddRange(items);
                    return;

                case ISet<T> set:
                    set.UnionWith(items);
                    return;
            }

            foreach (T item in items)
            {
                collection.Add(item);
            }
        }
    }
}
