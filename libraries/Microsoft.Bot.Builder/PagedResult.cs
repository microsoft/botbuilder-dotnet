// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Page of results from an enumeration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Page of items.
        /// </summary>
        public T[] Items { get; set; } = new T[0];

        /// <summary>
        /// Token used to page through multiple pages.
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}
