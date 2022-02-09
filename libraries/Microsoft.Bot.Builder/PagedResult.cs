// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Page of results from an enumeration.
    /// </summary>
    /// <typeparam name="T">The type of items in the results.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult{T}"/> class.
        /// </summary>
        /// <param name="items">The array of items.</param>
        public PagedResult(T[] items)
        {
            Items = new Collection<T>(items);
        }

        /// <summary>
        /// Gets the page of items.
        /// </summary>
        /// <value>
        /// The array of items.
        /// </value>
        public Collection<T> Items { get; } = new Collection<T>();

        /// <summary>
        /// Gets or sets a token for retrieving the next page of results.
        /// </summary>
        /// <value>
        /// The Continuation Token to pass to get the next page of results.
        /// </value>
        public string ContinuationToken { get; set; }
    }
}
