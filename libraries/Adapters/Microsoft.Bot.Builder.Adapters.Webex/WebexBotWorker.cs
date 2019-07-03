// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.BotKit;
using Microsoft.Bot.Builder.BotKit.Core;
using Microsoft.Bot.Schema;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    public class WebexBotWorker : BotWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebexBotWorker"/> class.
        /// </summary>
        /// <param name="botkit">The Botkit controller object responsible for spawning this bot worker.</param>
        /// <param name="config">Normally, a DialogContext object.</param>
        public WebexBotWorker(Botkit botkit, BotWorkerConfiguration config)
            : base(botkit, config)
        {
        }

        /// <summary>
        /// Gets or sets an instance of the [webex api client].
        /// </summary>
        /// <value>An instance of the webex api client.</value>
        public TeamsAPIClient Api { get; set; }

        /// <summary>
        /// Change the context of the _next_ message.
        /// Due to a quirk in the Webex API, we can't know the address of the DM until after sending the first message.
        /// As a result, the internal tracking for this conversation can't be persisted properly.
        /// USE WITH CAUTION while we try to sort this out.
        /// </summary>
        /// <param name="userId">User id of a webex teams user, like one from `message.user`.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> StartPrivateConversation(string userId)
        {
            var convRef = new ConversationReference();
            convRef.User.Id = userId;
            convRef.ChannelId = "webex";
            convRef.Conversation.Id = "temporary-value";

            return await this.ChangeContextAsync(convRef);
        }

        /// <summary>
        /// Switch a bot's context into a different room.
        /// After calling this method, messages sent with `bot.say` and any dialogs started with `bot.beginDialog` will occur in this new context.
        /// </summary>
        /// <param name="roomId">A Webex rooom id, like one found in `message.channel`.</param>
        /// <param name="userId">A Webex user id, like one found in `message.user`.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> StartConversationInRoom(string roomId, string userId)
        {
            var convRef = new ConversationReference();
            convRef.User.Id = userId;
            convRef.User.Name = null;
            convRef.ChannelId = "webex";
            convRef.Conversation.Id = roomId;

            return await this.ChangeContextAsync(convRef);
        }

        /// <summary>
        /// Deletes an existing message.
        /// </summary>
        /// <param name="update">An object in the form of `{id:-id of message to delete-}`.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteMessage(IBotkitMessage update)
        {
            var adapter = (WebexAdapter)this.Controller.Adapter;
            var context = this.Config.TurnContext;
            var source = new CancellationTokenSource();
            var token = source.Token;

            await adapter.DeleteActivityAsync(context, update.Reference, token).ConfigureAwait(false);
        }
    }
}
