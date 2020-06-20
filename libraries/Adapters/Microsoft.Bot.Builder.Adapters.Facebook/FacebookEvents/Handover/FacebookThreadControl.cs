// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover
{
    /// <summary>
    /// Base event object for thread control request payloads.
    /// </summary>
    public class FacebookThreadControl
    {
        /// <summary>
        /// Gets or sets the message sent from the requester.
        /// </summary>
        /// <remarks>
        /// Example: "All yours!".
        /// </remarks>
        /// <value>
        /// Message sent from the requester.
        /// </value>
        [JsonProperty("metadata")]
        public string Metadata { get; set; }
    }
}
