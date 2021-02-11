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
    }
}
