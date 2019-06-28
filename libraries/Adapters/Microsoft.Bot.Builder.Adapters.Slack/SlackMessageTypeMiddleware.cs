// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilder.Adapters.Slack
{
    /// <summary>
    /// A middleware for Botkit developers using the BotBuilder SlackAdapter class.
    /// This middleware causes Botkit to emit more specialized events for the different types of message that Slack might send.
    /// Responsible for classifying messages:
    ///      * `direct_message` events are messages received through 1:1 direct messages with the bot
    ///      * `direct_mention` events are messages that start with a mention of the bot, i.e "@mybot hello there"
    ///      * `mention` events are messages that include a mention of the bot, but not at the start, i.e "hello there @mybot"
    /// In addition, messages from bots and changing them to `bot_message` events. All other types of message encountered remain `message` events.
    /// </summary>
    public class SlackMessageTypeMiddleware : MiddlewareSet
    {
        /// <summary>
        /// Not for direct use - implements the MiddlewareSet's required onTurn function used to process the event.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="next">The next.</param>
        /// <param name="cancellationToken">The Cancellation Token.</param>
        public async void OnTurn(TurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.Activity.Type == "message" && context.Activity.ChannelData != null)
            {
                var adapter = context.Adapter as SlackAdapter;

                string botUserId = await adapter.GetBotUserByTeamAsync(context.Activity);
                var mentionSyntax = "<@" + botUserId + "(\\|.*?)?>";
                var mention = new Regex(mentionSyntax, RegexOptions.IgnoreCase);
                var directMention = new Regex('^' + mentionSyntax, RegexOptions.IgnoreCase).ToString();

                // is this a DM, a mention, or just ambient messages passing through?
                if ((context.Activity.ChannelData as dynamic)?.channel_type == "im")
                {
                    (context.Activity.ChannelData as dynamic).botkitEventType = "direct_message";

                    // strip the @mention
                    StripMention(context, directMention);
                }
                else if (!string.IsNullOrEmpty(botUserId) && !string.IsNullOrEmpty(context.Activity.Text) && context.Activity.Text.Equals(directMention))
                {
                    (context.Activity.ChannelData as dynamic).botkitEventType = "direct_mention";

                    // strip the @mention
                    StripMention(context, directMention);
                }
                else if (!string.IsNullOrEmpty(botUserId) && string.IsNullOrEmpty(context.Activity.Text) && context.Activity.Text.Equals(mention))
                {
                    (context.Activity.ChannelData as dynamic).botkitEventType = "mention";
                }

                // if this is a message from a bot, we probably want to ignore it.
                // switch the botkit event type to bot_message
                // and the activity type to Event <-- will stop it from being included in dialogs
                // NOTE: This catches any message from any bot, including this bot.
                // Note also, botId here is not the same as bot_user_id so we can't (yet) identify messages originating from this bot without doing an additional API call.
                if ((context.Activity.ChannelData as dynamic)?.botId != null)
                {
                    (context.Activity.ChannelData as dynamic).botkitEventType = "bot_message";
                    context.Activity.Type = ActivityTypes.Event;
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Strip any potential leading @mention.
        /// </summary>
        /// <param name="context">TurnContext to get the message text from.</param>
        /// <param name="directMention">Regex expression containing the direct mention format.</param>
        private void StripMention(TurnContext context, string directMention)
        {
            Regex.Replace(
                Regex.Replace(
                    Regex.Replace(
                        Regex.Replace(
                            context.Activity.Text,
                            directMention,
                            string.Empty),
                        @"/ ^\s +/",
                        string.Empty),
                    @"/ ^:\s +/",
                    string.Empty),
                @"/ ^\s +/",
                string.Empty);
        }
    }
}
