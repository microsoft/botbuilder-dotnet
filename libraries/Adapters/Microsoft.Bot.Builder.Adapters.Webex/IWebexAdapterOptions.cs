// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// Interface for defining implementation of the WebexAdapter Options.
    /// </summary>
    public interface IWebexAdapterOptions
    {
        /// <summary>
        /// Gets or sets an access token for the bot.
        /// </summary>
        /// <value>An access token for the bot. Get one from [https://developer.webex.com/](https://developer.webex.com/).</value>
        string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the secret used to validate incoming webhooks.
        /// </summary>
        /// <value>The secret used to validate incoming webhooks - you can define this yourself.</value>
        string Secret { get; set; }

        /// <summary>
        /// Gets or sets the root URL of your bot application.  Something like `https://mybot.com/`.
        /// </summary>
        /// <value>the root URL of your bot application.</value>
        string PublicAddress { get; set; }

        /// <summary>
        /// Gets or sets a name for the webhook subscription that will be created to tell Webex to send your bot webhooks.
        /// </summary>
        /// <value>A name for the webhook subscription.</value>
        string WebhookName { get; set; }
    }
}
