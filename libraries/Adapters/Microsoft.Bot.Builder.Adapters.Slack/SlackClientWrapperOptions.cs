// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    /// <summary>
    /// Class for defining implementation of the SlackClientWrapperOptions Options.
    /// </summary>
    public class SlackClientWrapperOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlackClientWrapperOptions"/> class.
        /// </summary>
        /// <param name="slackVerificationToken">A token for validating the origin of incoming webhooks.</param>
        /// <param name="slackBotToken">A token for a bot to work on a single workspace.</param>
        /// <param name="slackClientSigningSecret">The token used to validate that incoming webhooks are originated from Slack.</param>
        public SlackClientWrapperOptions(string slackVerificationToken, string slackBotToken, string slackClientSigningSecret)
        {
            SlackVerificationToken = slackVerificationToken;
            SlackBotToken = slackBotToken;
            SlackClientSigningSecret = slackClientSigningSecret;
        }

        /// <summary>
        /// Gets or Sets the token for validating the origin of incoming webhooks.
        /// </summary>
        /// <value>The verification token.</value>
        public string SlackVerificationToken { get; set; }

        /// <summary>
        /// Gets or Sets a token used to validate that incoming webhooks are originated from Slack.
        /// </summary>
        /// <value>The Client Signing Secret.</value>
        public string SlackClientSigningSecret { get; set; }

        /// <summary>
        /// Gets or Sets a token (provided by Slack) for a bot to work on a single workspace.
        /// </summary>
        /// <value>The Bot token.</value>
        public string SlackBotToken { get; set; }

        /// <summary>
        /// Gets or Sets the oauth client id provided by Slack for multi-team apps.
        /// </summary>
        /// <value>The Client Id.</value>
        public string SlackClientId { get; set; }

        /// <summary>
        /// Gets or Sets the oauth client secret provided by Slack for multi-team apps.
        /// </summary>
        /// <value>The Client Secret.</value>
        public string SlackClientSecret { get; set; }

        /// <summary>
        /// Gets or Sets the URI users will be redirected to after an oauth flow. In most cases, should be `https://mydomain.com/install/auth`.
        /// </summary>
        /// <value>The Redirect URI.</value>
        public Uri SlackRedirectUri { get; set; }

        /// <summary>
        /// Gets an array of scope names that are being requested during the oauth process. Must match the scopes defined at api.slack.com.
        /// </summary>
        /// <returns>The scopes array.</returns>
        /// <value>An array of scope names that are being requested during the oauth process.</value>
        public List<string> SlackScopes { get; } = new List<string>();
    }
}
