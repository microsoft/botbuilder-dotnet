// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotKit;
using Microsoft.Bot.Builder.BotKit.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SlackAPI;
using SlackAPI.RPCMessages;
using Dialog = SlackAPI.Dialog;

namespace Microsoft.BotBuilder.Adapters.Slack
{
    public class SlackBotWorker : BotWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlackBotWorker"/> class.
        /// Reserved for use internally by Botkit's `controller.spawn()`, this class is used to create a BotWorker instance that can send messages, replies, and make other API calls.
        /// When used with the SlackAdapter's multi-tenancy mode, it is possible to spawn a bot instance by passing in the Slack workspace ID of a team that has installed the app.
        /// Use this in concert with [startPrivateConversation()](#startPrivateConversation) and [changeContext()](core.md#changecontext) to start conversations
        /// or send proactive alerts to users on a schedule or in response to external events.
        /// </summary>
        /// <param name="botkit">The Botkit controller object responsible for spawning this bot worker.</param>
        /// <param name="config">Normally, a DialogContext object.  Can also be the id of a team.</param>
        public SlackBotWorker(Botkit botkit, BotWorkerConfiguration config)
            : base(botkit, config)
        {
            // TODO make overload to allow a teamid (string) to be passed in
        }

        public SlackTaskClient ApiClient { get; set; }

        /// <summary>
        /// Switch a bot's context to a 1:1 private message channel with a specific user.
        /// After calling this method, messages sent with `bot.say` and any dialogs started with `bot.beginDialog` will occur in this new context.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="userId">A Slack user id, like one found in `message.user` or in a `.<@mention>`</param>
        public async Task<object> StartPrivateConversation(string userId)
        {
            var channel = await this.ApiClient.JoinDirectMessageChannelAsync(userId);

            if (channel.ok)
            {
                var convRef = new ConversationReference();
                var activity = this.Config.Activity;

                convRef.Conversation.Id = channel.channel.id;
                (convRef.Conversation as dynamic).Team = (activity.Conversation as dynamic).Team; // TO-DO: replace 'as dynamic'
                convRef.User.Id = userId;
                convRef.User.Name = null;
                convRef.ChannelId = "slack";

                return this.ChangeContextAsync(convRef);
            }
            else
            {
                throw new Exception("Error creating IM channel: " + channel.ToString());
            }
        }

        /// <summary>
        /// Switch a bot's context into a different channel.
        /// After calling this method, messages sent with `bot.say` and any dialogs started with `bot.beginDialog` will occur in this new context.
        /// </summary>
        /// <param name="channelId">A Slack channel id, like one found in `message.channel`.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="userId">A Slack user id, like one found in `message.user` or in a `.<@mention>`</param>
        public async Task<object> StartConversationInChannel(string channelId, string userId)
        {
            var convRef = new ConversationReference();
            var activity = this.Config.Activity;

            convRef.Conversation.Id = channelId;
            (convRef.Conversation as dynamic).Team = (activity.Conversation as dynamic).Team; // TO-DO: replace 'as dynamic'
            convRef.User.Id = userId;
            convRef.User.Name = null;
            convRef.ChannelId = "slack";

            return await this.ChangeContextAsync(convRef);
        }

        /// <summary>
        /// Switch a bot's context into a specific sub-thread within a channel.
        /// After calling this method, messages sent with `bot.say` and any dialogs started with `bot.beginDialog` will occur in this new context.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="channelId">A Slack channel id, like one found in `message.channel`.</param>
        /// <param name="userId">A Slack user id, like one found in `message.user` or in a '.@mention'.</param>
        /// <param name="threadTs">A thread_ts value found in the `message.thread_ts` or `message.ts` field.</param>
        public async Task<object> StartConversationInThread(string channelId, string userId, string threadTs)
        {
            var convRef = new ConversationReference();
            var activity = this.Config.Activity;

            convRef.Conversation.Id = channelId;
            (convRef.Conversation as dynamic).Team = (activity.Conversation as dynamic).Team; // TO-DO: replace 'as dynamic'
            (convRef.Conversation as dynamic).ThreadTS = threadTs; // TO-DO: replace 'as dynamic'
            convRef.User.Id = userId;
            convRef.User.Name = null;
            convRef.ChannelId = "slack";

            return await this.ChangeContextAsync(convRef);
        }

        /// <summary>
        /// Like bot.reply, but as a threaded response to the incoming message rather than a new message in the main channel.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="source">An incoming message object.</param>
        /// <param name="resp">An outgoing message object (or part of one or just reply text).</param>
        public async Task<object> ReplyInThread(BotkitSlackMessage source, BotkitSlackMessage resp)
        {
            // make sure the  threadTs setting is set
            // this will be included in the conversation reference
            var threadTs = (source.IncomingMessage.ChannelData as dynamic).ThreadTs; // TO-DO: replace 'as dynamic'
            if (threadTs is null)
            {
                threadTs = (source.IncomingMessage.ChannelData as dynamic).Ts; // TO-DO: replace 'as dynamic'
            }

            return await this.ReplyAsync(source, resp);
        }

        /// <summary>
        /// Like bot.reply, but sent as an "ephemeral" message meaning only the recipient can see it.
        /// Uses chat.postEphemeral.
        /// </summary>
        /// <param name="source">An incoming message object.</param>
        /// <param name="resp">An outgoing message object (or part of one or just reply text).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> ReplyEphemeral(BotkitSlackMessage source, BotkitSlackMessage resp)
        {
            // make sure resp is in an object format.
            var activity = this.EnsureMessageFormat(resp);

            // make sure ephemeral is set
            // fields set in channelData will end up in the final message to slack
            (activity.ChannelData as dynamic).ephemeral = true; // TO-DO: replace 'as dynamic'

            resp.IncomingMessage = activity;

            return await this.ReplyAsync(source, resp);
        }

        /// <summary>
        /// Like bot.reply, but used to send an immediate public reply to a /slash command.
        /// The message in `resp` will be displayed to everyone in the channel.
        /// </summary>
        /// <param name="source">An incoming message object of type `slash_command`.</param>
        /// <param name="resp">An outgoing message object (or part of one or just reply text).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> ReplyPublic(BotkitSlackMessage source, BotkitSlackMessage resp)
        {
            var activity = this.EnsureMessageFormat(resp);

            (activity.ChannelData as dynamic).ResponseType = "in_channel"; // TO-DO: replace 'as dynamic'

            resp.IncomingMessage = activity;

            return await this.ReplyInteractive(source, resp);
        }

        /// <summary>
        /// Like bot.reply, but used to send an immediate private reply to a /slash command.
        /// The message in `resp` will be displayed only to the person who executed the slash command.
        /// </summary>
        /// <param name="source">An incoming message object of type `slash_command`.</param>
        /// <param name="resp">An outgoing message object (or part of one or just reply text).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> ReplyPrivate(BotkitSlackMessage source, BotkitSlackMessage resp)
        {
            var activity = this.EnsureMessageFormat(resp);

            (activity.ChannelData as dynamic).ResponseType = "ephemeral"; // TO-DO: replace 'as dynamic'
            (activity.ChannelData as dynamic).To = source.User; // TO-DO: replace 'as dynamic'

            resp.IncomingMessage = activity;

            return await this.ReplyInteractive(source, resp);
        }

        /// <summary>
        /// Like bot.reply, but used to respond to an `interactive_message` event and cause the original message to be replaced with a new one.
        /// An incoming message object of type `interactive_message`.
        /// </summary>
        /// <param name="source">An incoming message object of type `interactive_message`.</param>
        /// <param name="resp">A new or modified message that will replace the original one.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> ReplyInteractive(BotkitSlackMessage source, BotkitSlackMessage resp)
        {
            // TO-DO: replace 'as dynamic'
            if ((source.IncomingMessage.ChannelData as dynamic).ResponseUrl is null)
            {
                throw new Exception("No responseUrl found in incoming message");
            }

            var activity = this.EnsureMessageFormat(resp);

            activity.Conversation.Id = source.Channel;

            (activity.ChannelData as dynamic).To = source.User; // TO-DO: replace 'as dynamic'

            // TO-DO: replace 'as dynamic'
            if ((source.IncomingMessage.ChannelData as dynamic).ThreadTs != null)
            {
                (activity.Conversation as dynamic).ThreadTs = (source.IncomingMessage.ChannelData as dynamic).ThreadTs; // TO-DO: replace 'as dynamic'
            }

            var adapter = (SlackAdapter)this.Controller.Adapter;

            var slackMessage = adapter.ActivityToSlack(activity);

            var requestOptions = new
            {
                uri = (source.IncomingMessage.ChannelData as dynamic).ResponseUrl, // TO-DO: replace 'as dynamic'
                method = "POST",
                json = slackMessage,
            };

            return await this.RequestUrl(requestOptions);
        }

        public async Task<object> RequestUrl(dynamic options)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsync(options.uri, options.json);
                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject(result);
            }
        }

        /// <summary>
        /// Return 1 or more error to a `dialog_submission` event that will be displayed as form validation errors.
        /// Each error must be mapped to the name of an input in the dialog.
        /// </summary>
        /// <param name="error">1 or more objects in form {name: string, error: string}.</param>
        public void DialogError(AdapterError error)
        {
            if (error != null)
            {
                this.HTTPBody(JsonConvert.ToString(error));
            }
        }

        public void DialogError(AdapterError[] errors)
        {
            if (errors != null)
            {
                this.HTTPBody(JsonConvert.ToString(errors));
            }
        }

        /// <summary>
        /// Reply to a button click with a request to open a dialog.
        /// </summary>
        /// <param name="source">An incoming `interactive_callback` event containing a `trigger_id` field.</param>
        /// <param name="dialogObj">A dialog, as created using [SlackDialog](#SlackDialog) or [authored to this spec](https://api.slack.com/dialogs).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogOpenResponse> ReplyWithDialog(object source, object dialogObj)
        {
            var triggerId = (source as dynamic).TriggerId;
            var dialog = (Dialog)dialogObj;

            return await this.ApiClient.DialogOpenAsync(triggerId, dialog);
        }

        /// <summary>
        /// Update an existing message with new content.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="update">An object in the form `{id: {id of message to update}, conversation: { id: [channel] }, text: [new text], card: [array of card objects]}`.</param>
        public async Task<ResourceResponse> UpdateMessage(IBotkitMessage update)
        {
            SlackAdapter adapter = (SlackAdapter)this.Controller.Adapter;
            TurnContext context = this.Config.TurnContext;
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            return await adapter.UpdateActivityAsync(context, update.IncomingMessage, token);
        }

        /// <summary>
        /// Delete an existing message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="update">An object in the form of {id:[id of message to delete], conversation: { id: [channel of message] }}.</param>
        public async Task DeleteMessage(IBotkitMessage update)
        {
            var adapter = (BotFrameworkAdapter)this.Controller.Adapter;
            var context = this.Config.TurnContext;
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            await adapter.DeleteActivityAsync(context, update.Reference, token);
        }
    }
}
