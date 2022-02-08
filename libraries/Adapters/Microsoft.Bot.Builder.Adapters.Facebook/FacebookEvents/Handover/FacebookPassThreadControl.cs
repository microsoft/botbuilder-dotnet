// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover
{
    /// <summary>
    /// Event object sent to Facebook when requesting to pass thread control to another app.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class FacebookPassThreadControl : FacebookThreadControl
    {
        /// <summary>
        /// Gets or sets the app ID of the new owner.
        /// </summary>
        /// <remarks>
        /// 263902037430900 for the page inbox.
        /// </remarks>
        /// <value>The app ID of the new owner.</value>
        [JsonProperty("new_owner_app_id")]
        public string NewOwnerAppId { get; set; }

        /// <summary>
        /// Gets or sets the app ID of the previous owner.
        /// </summary>
        /// <value>The app ID of the previous owner.</value>
        [JsonProperty("previous_owner_app_id")]
        public string PreviousOwnerAppId { get; set; }

        /// <summary>
        /// Gets or sets the app ID of the requested owner.
        /// </summary>
        /// <value>The app ID of the requested owner.</value>
        [JsonProperty("requested_owner_app_id")]
        public string RequestedOwnerAppId { get; set; }
    }
}
