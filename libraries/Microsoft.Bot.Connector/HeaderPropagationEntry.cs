// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Represents the action of the header entry.
    /// </summary>
    public enum HeaderPropagationEntryAction
    {
        /// <summary>
        /// Adds a new header entry to the outgoing request.
        /// </summary>
        [EnumMember(Value = "add")]
        Add,

        /// <summary>
        /// Appends a new header value to an existing key in the outgoing request.
        /// </summary>
        [EnumMember(Value = "append")]
        Append,

        /// <summary>
        /// Propagates the header entry from the incoming request to the outgoing request without modifications.
        /// </summary>
        [EnumMember(Value = "propagate")]
        Propagate,

        /// <summary>
        /// Overrides an existing header entry in the outgoing request.
        /// </summary>
        [EnumMember(Value = "override")]
        Override
    }

    /// <summary>
    /// Represents a single header entry used for header propagation.
    /// </summary>
    public class HeaderPropagationEntry
    {
        /// <summary>
        /// Gets or sets the key of the header entry.
        /// </summary>
        /// <value>Key of the header entry.</value>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the header entry.
        /// </summary>
        /// <value>Value of the header entry.</value>
        public StringValues Value { get; set; } = new StringValues(string.Empty);

        /// <summary>
        /// Gets or sets the action of the header entry (Add, Append, Override or Propagate).
        /// </summary>
        /// <value>Action of the header entry.</value>
        public HeaderPropagationEntryAction Action { get; set; } = HeaderPropagationEntryAction.Propagate;
    }
}
