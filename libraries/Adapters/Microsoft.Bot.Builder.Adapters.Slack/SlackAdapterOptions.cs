// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    /// <summary>
    /// Class for defining implementation of the SlackAdapter Options.
    /// </summary>
    public class SlackAdapterOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the signatures of incoming requests should be verified.
        /// </summary>
        /// <value>
        /// A value indicating whether the signatures of incoming requests should be verified.
        /// </value>
        public bool VerifyIncomingRequests { get; set; } = true;

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
