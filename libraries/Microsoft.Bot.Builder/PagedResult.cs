// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

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
        /// <value>
        /// The array of items.
        /// </value>
#pragma warning disable CA1819 // Properties should not return arrays (can't change this without breaking binary compat)
        public T[] Items { get; set; } = Array.Empty<T>();
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets a token for retrieving the next page of results.
        /// </summary>
        /// <value>
        /// The Continuation Token to pass to get the next page of results.
        /// </value>
        public string ContinuationToken { get; set; }
    }
}
