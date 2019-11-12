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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "it needs to be set in ActivityToFacebook method")]
        public List<FacebookEntry> Entry { get; set; }
    }
}
