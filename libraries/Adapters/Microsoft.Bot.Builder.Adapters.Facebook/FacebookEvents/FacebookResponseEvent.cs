// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookResponseEvent
    {
        [JsonProperty(PropertyName = "object")]
        public string ResponseObject { get; set; }

        public List<FacebookEntry> Entry { get; } = new List<FacebookEntry>();
    }
}
