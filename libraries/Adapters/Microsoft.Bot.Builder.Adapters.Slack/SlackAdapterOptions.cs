// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    /// <summary>
    /// Class for defining implementation of the SlackAdapter Options.
    /// </summary>
    public class SlackAdapterOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlackAdapterOptions"/> class.
        /// </summary>
        /// <param name="slackVerificationToken">A token for validating the origin of incoming webhooks.</param>
        /// <param name="slackBotToken">A token for a bot to work on a single workspace.</param>
        /// <param name="slackClientSigningSecret">The token used to validate that incoming webhooks are originated from Slack.</param>
        public SlackAdapterOptions(string slackVerificationToken, string slackBotToken, string slackClientSigningSecret)
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
        /// Gets or sets an array of scope names that are being requested during the oauth process. Must match the scopes defined at api.slack.com.
        /// </summary>
        /// <returns>The scopes array.</returns>
        public List<string> SlackScopes { get; set; } = new List<string>();

        /// <summary>
        /// A method that receives a Slack team id and returns the bot token associated with that team. Required for multi-team apps.
        /// </summary>
        /// <param name="teamId">Team ID.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the task.</param>
        /// <returns>The bot token associated with the team.</returns>
        public Task<string> GetTokenForTeamAsync(string teamId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A method that receives a Slack team id and returns the bot user id associated with that team. Required for multi-team apps.
        /// </summary>
        /// <param name="teamId">Team ID.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the task.</param>
        /// <returns>The bot user id associated with that team.</returns>
        public virtual Task<string> GetBotUserByTeamAsync(string teamId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
