﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Options for the <see cref="TwilioAdapter"/>.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class TwilioAdapterOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether incoming requests should be validated as coming from Twilio.
        /// </summary>
        /// <value>
        /// A value indicating whether incoming requests should be validated as coming from Twilio.
        /// </value>
        public bool ValidateIncomingRequests { get; set; } = true;
    }
}
