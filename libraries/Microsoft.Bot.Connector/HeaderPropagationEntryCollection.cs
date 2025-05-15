// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

#pragma warning disable SA1010 // OpeningSquareBracketsMustBeSpacedCorrectly

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Represents a collection of all the header entries configured to be propagated to outgoing requests.
    /// </summary>
    public class HeaderPropagationEntryCollection
    {
        private readonly Dictionary<string, HeaderPropagationEntry> _entries = [];

        /// <summary>
        /// Gets the collection of header entries to be propagated to outgoing requests.
        /// </summary>
        /// <value>The collection of header entries.</value>
        public List<HeaderPropagationEntry> Entries => [.. _entries.Select(x => x.Value)];

        /// <summary>
        /// Attempts to add a new header entry to the collection.
        /// </summary>
        /// <remarks>
        /// If the key already exists, it will be ignored.
        /// </remarks>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value to add for the specified key.</param>
        public void Add(string key, StringValues value)
        {
            _entries[key] = new HeaderPropagationEntry
            {
                Key = key,
                Value = value,
                Action = HeaderPropagationEntryAction.Add
            };
        }

        /// <summary>
        /// Appends a new header value to an existing key.
        /// </summary>
        /// <remarks>
        /// If the key does not exist, it will be ignored.
        /// </remarks>
        /// <param name="key">The key of the element to append the value.</param>
        /// <param name="value">The value to append for the specified key.</param>
        public void Append(string key, StringValues value)
        {
            _entries[key] = new HeaderPropagationEntry
            {
                Key = key,
                Value = value,
                Action = HeaderPropagationEntryAction.Append
            };
        }

        /// <summary>
        /// Propagates the incoming request header value to outgoing requests without modifications.
        /// </summary>
        /// <remarks>
        /// If the key does not exist, it will be ignored.
        /// </remarks>
        /// <param name="key">The key of the element to propagate.</param>
        public void Propagate(string key)
        {
            _entries[key] = new HeaderPropagationEntry
            {
                Key = key,
                Action = HeaderPropagationEntryAction.Propagate
            };
        }

        /// <summary>
        /// Overrides the header value of an existing key.
        /// </summary>
        /// <remarks>
        /// If the key does not exist, it will add it.
        /// </remarks>
        /// <param name="key">The key of the element to override.</param>
        /// <param name="value">The value to override in the specified key.</param>
        public void Override(string key, StringValues value)
        {
            _entries[key] = new HeaderPropagationEntry
            {
                Key = key,
                Value = value,
                Action = HeaderPropagationEntryAction.Override
            };
        }
    }
}
 
#pragma warning restore SA1010 // OpeningSquareBracketsMustBeSpacedCorrectly
