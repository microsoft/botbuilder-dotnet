// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// Defines implementation of the WebexAdapter Options.
    /// </summary>
    public class WebexAdapterOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebexAdapterOptions"/> class.
        /// </summary>
        /// <param name="accessToken">An access token for the bot.</param>
        /// <param name="publicAddress">The root URL of the bot application.</param>
        /// <param name="secret">The secret used to validate incoming webhooks.</param>
        /// <param name="webhookName">A name for the webhook subscription.</param>
        public WebexAdapterOptions(string accessToken, Uri publicAddress, string secret, string webhookName = null)
        {
            AccessToken = accessToken;
            PublicAddress = publicAddress;
            Secret = secret;
            WebhookName = webhookName;
        }

        /// <summary>
        /// Gets or sets an access token for the bot.
        /// </summary>
        /// <value>An access token for the bot. Get one from https://developer.webex.com/.</value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the secret used to validate incoming webhooks.
        /// </summary>
        /// <value>The secret used to validate incoming webhooks. You can define this yourself.</value>
        public string Secret { get; set; }

        /// <summary>
        /// Gets or sets the root URI of your bot application. Something like 'https://mybot.com/'.
        /// </summary>
        /// <value>the root URI of your bot application.</value>
        public Uri PublicAddress { get; set; }

        /// <summary>
        /// Gets or sets a name for the webhook subscription that will be created to tell WebEx to send your bot webhooks.
        /// </summary>
        /// <value>A name for the webhook subscription.</value>
        public string WebhookName { get; set; }
    }
}
