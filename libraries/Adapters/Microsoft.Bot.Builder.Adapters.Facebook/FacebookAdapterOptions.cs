// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    /// <summary>
    /// Options class for Facebook Adapter.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class FacebookAdapterOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether incoming requests should be verified.
        /// Should be set to true in Production but can be set to false for testing or development purposes.
        /// </summary>
        /// <value>The flag to indicate if incoming requests should be verified.</value>
        public bool VerifyIncomingRequests { get; set; } = true;
    }
}
