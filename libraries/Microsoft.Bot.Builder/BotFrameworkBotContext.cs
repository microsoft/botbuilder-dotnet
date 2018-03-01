// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Extended bot context adding fields which are only valid for BotFramework.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.BotContext" />
    public class BotFrameworkBotContext : BotContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkBotContext"/> class.
        /// </summary>
        /// <param name="botAppId">The bot application identifier. Bot's AAD identity.</param>
        /// <param name="botAdapter">The bot adapter.</param>
        /// <param name="requestActivity">The request activity.</param>
        public BotFrameworkBotContext(string botAppId, BotAdapter botAdapter, Activity requestActivity)
            : base(botAdapter, requestActivity)
        {
            this.BotAppId = botAppId;
        }

        /// <summary>
        /// Gets or sets the bot application identifier. The unqiue Id by which the bot is recognized in AAD.
        /// </summary>
        public string BotAppId { get; }
    }
}
