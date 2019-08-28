// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Tracking information for a dialog on the stack.
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public class DialogInstance
    {
        /// <summary>
        /// Gets or sets the ID of the dialog this instance is for.
        /// </summary>
        /// <value>
        /// ID of the dialog this instance is for.
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the instance's persisted state.
        /// </summary>
        /// <value>
        /// The instance's persisted state.
        /// </value>
        [JsonProperty("state")]
        public IDictionary<string, object> State { get; set; }

        /// <summary>
        /// Gets or sets a stack index. Positive values are indexes within the current DC and negative values are 
        /// indexes in the parent DC.
        /// </summary>
        /// <value>
        /// Positive values are indexes within the current DC and negative values are indexes in
        /// the parent DC.
        /// </value>
        public int? StackIndex { get; set; }
    }
}
