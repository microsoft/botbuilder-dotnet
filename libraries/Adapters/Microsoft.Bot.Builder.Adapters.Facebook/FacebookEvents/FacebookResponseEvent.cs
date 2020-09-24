// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Represents the incoming object received from Facebook and processed by the adapter.
    /// </summary>
    public class FacebookResponseEvent
    {
        /// <summary>
        /// Gets or sets the object property.
        /// </summary>
        /// <value>A string with different values.</value>
        [JsonProperty(PropertyName = "object")]
        public string ResponseObject { get; set; }

        /// <summary>
        /// Gets the entry property.
        /// </summary>
        /// <value>Array containing event data.</value>
        public List<FacebookEntry> Entry { get; } = new List<FacebookEntry>();

        /// <summary>
        /// Gets the flag to determine if the Entry property should be serialized.
        /// </summary>
        /// <returns>True if Entry count is greater than zero.</returns>
        public bool ShouldSerializeEntry()
        {
            return Entry.Count > 0;
        }
    }
}
