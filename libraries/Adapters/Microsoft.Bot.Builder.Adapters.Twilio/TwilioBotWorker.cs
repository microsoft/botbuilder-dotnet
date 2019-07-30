// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.BotKit;
using Microsoft.Bot.Builder.BotKit.Core;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public class TwilioBotWorker : BotWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioBotWorker"/> class.
        /// </summary>
        /// <param name="botkit">The Botkit controller object responsible for spawning this bot worker.</param>
        /// <param name="config">Normally, a DialogContext object.</param>
        public TwilioBotWorker(Botkit botkit, BotWorkerConfiguration config)
            : base(botkit, config)
        {
        }

        /// <summary>
        /// Gets or sets an instance of the [webex api client].
        /// </summary>
        /// <value>An instance of the twilio api client.</value>
        public object Api { get; set; }

        /// <summary>
        /// Start a conversation with a given user identified by their phone number. Useful for sending pro-active messages.
        /// </summary>
        /// <param name="userID">userId A phone number in the form +1XXXYYYZZZZ.</param>
        /// <returns>A task representing the async operation.</returns>
        public async Task StartConversationWithUserAsync(string userID)
        {
            var userChannelAccount = new ChannelAccount(userID);
            var botChannelAccount = new ChannelAccount(this.Controller.GetConfig("twilio_number").ToString(), "bot");
            var conversation = new ConversationAccount(null, null, userID);
            var conversationReference = new ConversationReference(null, userChannelAccount, botChannelAccount, conversation, "twilio-sms");

            await this.ChangeContextAsync(conversationReference);
        }
    }
}
