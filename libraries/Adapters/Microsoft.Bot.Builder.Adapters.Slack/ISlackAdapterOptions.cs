// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.BotBuilder.Adapters.Slack
{
    /// <summary>
    /// Interface for defining implementation of the SlackAdapter Options.
    /// </summary>
    public interface ISlackAdapterOptions
    {
        /// <summary>
        /// Gets or Sets the token for validating the origin of incoming webhooks.
        /// </summary>
        /// <value>The verification token.</value>
        string VerificationToken { get; set; }

        /// <summary>
        /// Gets or Sets a token used to validate that incoming webhooks originated with Slack.
        /// </summary>
        /// <value>The Client Signing Secret.</value>
        string ClientSigningSecret { get; set; }

        /// <summary>
        /// Gets or Sets a token (provided by Slack) for a bot to work on a single workspace.
        /// </summary>
        /// <value>The Bot token.</value>
        string BotToken { get; set; }

        /// <summary>
        /// Gets or Sets the oauth client id provided by Slack for multi-team apps.
        /// </summary>
        /// <value>The Client Id.</value>
        string ClientId { get; set; }

        /// <summary>
        /// Gets or Sets the oauth client secret provided by Slack for multi-team apps.
        /// </summary>
        /// <value>The Client Secret.</value>
        string ClientSecret { get; set; }

        /// <summary>
        /// Gets or Sets an array of scope names that are being requested during the oauth process. Must match the scopes defined at api.slack.com.
        /// </summary>
        /// <value>The Scopes array.</value>
        string[] Scopes { get; set; }

        /// <summary>
        /// Gets or Sets the URI users will be redirected to after an oauth flow. In most cases, should be `https://mydomain.com/install/auth`.
        /// </summary>
        /// <value>The Redirect URI.</value>
        string RedirectUri { get; set; }

        /// <summary>
        /// A method that receives a Slack team id and returns the bot token associated with that team. Required for multi-team apps.
        /// </summary>
        /// <param name="teamId">Team ID.</param>
        /// <returns>The bot token associated with the team.</returns>
        Task<string> GetTokenForTeam(string teamId);

        /// <summary>
        /// A method that receives a Slack team id and returns the bot user id associated with that team. Required for multi-team apps.
        /// </summary>
        /// <param name="teamId">Team ID.</param>
        /// <returns>The bot user id associated with that team.</returns>
        Task<string> GetBotUserByTeam(string teamId);
    }
}
