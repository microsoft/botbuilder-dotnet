// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
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

        public bool ShouldSerializeEntry()
        {
            return Entry.Count > 0;
        }
    }
}
