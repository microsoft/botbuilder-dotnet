// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Page of results from an enumeration.
    /// </summary>
    /// <typeparam name="T">The type of items in the results.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the page of items.
        /// </summary>
        public T[] Items { get; set; } = new T[0];

        /// <summary>
        /// Gets or sets a token for retrieving the next page of results.
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}
