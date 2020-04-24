// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Contains state information associated with a <see cref="Dialog"/> on a dialog stack.
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public class DialogInstance
    {
        /// <summary>
        /// Gets or sets the ID of the dialog.
        /// </summary>
        /// <value>
        /// The ID of the dialog.
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

        /// <summary>
        /// Gets or sets version string.
        /// </summary>
        /// <value>Unique string from the dialog this dialoginstance is tracking which is used to identify when a dialog has changed in way that should emit an event for changed content.</value>
        public string Version { get; set; }
    }
}
