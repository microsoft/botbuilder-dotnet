// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover
{
    /// <summary>A Facebook thread control message, including appid of requested thread owner and an optional message to send with the request.</summary>
    public class FacebookRequestThreadControl : FacebookThreadControl
    {
        /// <summary>
        /// Gets or Sets the app id of the requested owner.
        /// </summary>
        /// <remarks>
        /// 263902037430900 for the page inbox.
        /// </remarks>
        /// <value>
        /// the app id of the requested owner.
        /// </value>
        [JsonProperty("requested_owner_app_id")]
        public string RequestedOwnerAppId { get; set; }
    }
}
