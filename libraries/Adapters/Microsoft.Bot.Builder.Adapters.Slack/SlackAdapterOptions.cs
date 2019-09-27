// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
        /// <param name="verificationToken">A token for validating the origin of incoming webhooks.</param>
        /// <param name="botToken">A token for a bot to work on a single workspace.</param>
        /// <param name="clientSigningSecret">The token used to validate that incoming webhooks are originated from Slack.</param>
        public SlackAdapterOptions(string verificationToken, string botToken, string clientSigningSecret)
        {
            VerificationToken = verificationToken;
            BotToken = botToken;
            ClientSigningSecret = clientSigningSecret;
        }

        /// <summary>
        /// Gets or Sets the token for validating the origin of incoming webhooks.
        /// </summary>
        /// <value>The verification token.</value>
        public string VerificationToken { get; set; }

        /// <summary>
        /// Gets or Sets a token used to validate that incoming webhooks are originated from Slack.
        /// </summary>
        /// <value>The Client Signing Secret.</value>
        public string ClientSigningSecret { get; set; }

        /// <summary>
        /// Gets or Sets a token (provided by Slack) for a bot to work on a single workspace.
        /// </summary>
        /// <value>The Bot token.</value>
        public string BotToken { get; set; }

        /// <summary>
        /// Gets or Sets the oauth client id provided by Slack for multi-team apps.
        /// </summary>
        /// <value>The Client Id.</value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or Sets the oauth client secret provided by Slack for multi-team apps.
        /// </summary>
        /// <value>The Client Secret.</value>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or Sets the URI users will be redirected to after an oauth flow. In most cases, should be `https://mydomain.com/install/auth`.
        /// </summary>
        /// <value>The Redirect URI.</value>
        public Uri RedirectUri { get; set; }

        /// <summary>
        /// A method that returns an array of scope names that are being requested during the oauth process. Must match the scopes defined at api.slack.com.
        /// </summary>
        /// <returns>The scopes array.</returns>
        public string[] GetScopes()
        {
            throw new NotImplementedException();
        }

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
