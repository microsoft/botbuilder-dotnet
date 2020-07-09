// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// Options class for the <see cref="WebexAdapter" />.
    /// </summary>
    public class WebexAdapterOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the signature on incoming requests should be validated as originating from Webex.
        /// </summary>
        /// <value>
        /// A value indicating if the signature on incoming requests should be validated as originating from Webex.
        /// </value>
        public bool ValidateIncomingRequests { get; set; } = true;

        /// <summary>
        /// Gets or sets a value for an Id used to represent your bot application and
        /// it should be consistent across all adapters. If you are using Azure Bot Service
        /// channels then you should use your MicrosoftAppId.
        /// </summary>
        /// <value>
        /// A value for an Id used to represent your bot application and
        /// it should be consistent across all adapters. If you are using Azure Bot Service
        /// channels then you should use your MicrosoftAppId.
        /// </value>
        public string AppId { get; set; } = null;
    }
}
