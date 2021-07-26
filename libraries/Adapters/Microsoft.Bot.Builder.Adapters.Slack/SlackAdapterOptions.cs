// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

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
        /// Gets or sets a delegate to retrieve a bot token for a given workspace id.
        /// </summary>
        /// <value>
        /// The delegate that retrieves a bot token for a given workspace id.
        /// </value>
        public Func<string, Task<string>> GetTokenForWorkspace { get; set; }

        /// <summary>
        /// Gets or sets a delegate to retrieve a bot user id for a given workspace id.
        /// </summary>
        /// <value>
        /// The delegate that retrieves a bot user id for a given workspace id.
        /// </value>
        public Func<string, Task<string>> GetBotUserIdentity { get; set; }
    }
}
