// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// Defines implementation of the WebexAdapter Options.
    /// </summary>
    public class WebexClientWrapperOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebexClientWrapperOptions"/> class.
        /// </summary>
        /// <param name="webexAccessToken">An access token for the bot.</param>
        /// <param name="webexPublicAddress">The root URL of the bot application.</param>
        /// <param name="webexSecret">The secret used to validate incoming webhooks.</param>
        /// <param name="webexWebhookName">A name for the webhook subscription.</param>
        public WebexClientWrapperOptions(string webexAccessToken, Uri webexPublicAddress, string webexSecret, string webexWebhookName = null)
        {
            WebexAccessToken = webexAccessToken;
            WebexPublicAddress = webexPublicAddress;
            WebexSecret = webexSecret;
            WebexWebhookName = webexWebhookName;
        }

        /// <summary>
        /// Gets or sets an access token for the bot.
        /// </summary>
        /// <value>An access token for the bot. Get one from 'https://developer.webex.com/'.</value>
        public string WebexAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the secret used to validate incoming webhooks.
        /// </summary>
        /// <value>The secret used to validate incoming webhooks. You can define this yourself.</value>
        public string WebexSecret { get; set; }

        /// <summary>
        /// Gets or sets the root URI of your bot application. Something like 'https://mybot.com/'.
        /// </summary>
        /// <value>the root URI of your bot application.</value>
        public Uri WebexPublicAddress { get; set; }

        /// <summary>
        /// Gets or sets a name for the webhook subscription that will be created to tell Webex to send your bot webhooks.
        /// </summary>
        /// <value>A name for the webhook subscription.</value>
        public string WebexWebhookName { get; set; }
    }
}
