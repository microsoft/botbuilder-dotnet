// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover
{
    /// <summary>
    /// Event object sent to Facebook when requesting to take thread control from another app.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class FacebookTakeThreadControl : FacebookThreadControl
    {
        /// <summary>
        /// Gets or sets the app ID of the previous owner.
        /// </summary>
        /// <remarks>
        /// 263902037430900 for the page inbox.
        /// </remarks>
        /// <value>
        /// The app ID of the previous owner.
        /// </value>
        [JsonProperty("previous_owner_app_id")]
        public string PreviousOwnerAppId { get; set; }
    }
}
