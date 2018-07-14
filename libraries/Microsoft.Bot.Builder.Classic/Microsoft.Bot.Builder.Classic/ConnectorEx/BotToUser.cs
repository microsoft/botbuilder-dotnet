// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.ConnectorEx;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    /// <summary>
    /// Methods to send a message from the bot to the user. 
    /// </summary>
    public interface IBotToUser
    {
        /// <summary>
        /// Post a message to be sent to the user.
        /// </summary>
        /// <param name="message">The message for the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        Task PostAsync(IMessageActivity message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Make a message.
        /// </summary>
        /// <returns>The new message.</returns>
        IMessageActivity MakeMessage();
    }

    public sealed class NullBotToUser : IBotToUser
    {
        private readonly IMessageActivity toBot;
        public NullBotToUser(IMessageActivity toBot)
        {
            SetField.NotNull(out this.toBot, nameof(toBot), toBot);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            var toBotActivity = (Activity)this.toBot;
            return toBotActivity.CreateReply();
        }

        Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public sealed class PassBotToUser : IBotToUser
    {
        private readonly IBotToUser inner;
        public PassBotToUser(IBotToUser inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            return this.inner.MakeMessage();
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.inner.PostAsync(message, cancellationToken);
        }
    }

    public sealed class AlwaysSendDirect_BotToUser : IBotToUser
    {
        private readonly IMessageActivity toBot;
        private readonly IConnectorClient client;
        public AlwaysSendDirect_BotToUser(IMessageActivity toBot, IConnectorClient client)
        {
            SetField.NotNull(out this.toBot, nameof(toBot), toBot);
            SetField.NotNull(out this.client, nameof(client), client);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            var toBotActivity = (Activity)this.toBot;
            return toBotActivity.CreateReply();
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.client.Conversations.ReplyToActivityAsync((Activity)message, cancellationToken);
        }
    }

    public sealed class V4Bridge_BotToUser : IBotToUser
    {
        private readonly Microsoft.Bot.Builder.ITurnContext context;

        public V4Bridge_BotToUser(Microsoft.Bot.Builder.ITurnContext context)
        {
            SetField.NotNull(out this.context, nameof(context), context);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            var toBotActivity = (Activity)this.context.Activity;
            return toBotActivity.CreateReply();
        }

        Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            // TODO, change this to context.SendActivity with M2 delta
            return this.context.Adapter.SendActivitiesAsync(this.context, new Activity[] { (Activity) message }, cancellationToken);
        }
    }

    public interface IMessageQueue
    {
        Task QueueMessageAsync(IBotToUser botToUser, IMessageActivity message, CancellationToken token);
        Task DrainQueueAsync(IBotToUser botToUser, CancellationToken token);
    }

    public sealed class AutoInputHint_BotToUser : IBotToUser
    {
        private readonly IBotToUser inner;
        private readonly IMessageQueue queue;

        public AutoInputHint_BotToUser(IBotToUser inner, IMessageQueue queue)
        {
            SetField.NotNull(out this.queue, nameof(queue), queue);
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.queue.QueueMessageAsync(inner, message, cancellationToken);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            return inner.MakeMessage();
        }
    }

    public sealed class InputHintQueue : IMessageQueue
    {
        private readonly Queue<IMessageActivity> queue = new Queue<IMessageActivity>();
        private readonly IChannelCapability channelCapability;
        private readonly Func<IDialogStack> makeStack;

        public InputHintQueue(IChannelCapability channelCapability, Func<IDialogStack> makeStack)
        {
            SetField.NotNull(out this.channelCapability, nameof(channelCapability), channelCapability);
            SetField.NotNull(out this.makeStack, nameof(makeStack), makeStack);
        }

        async Task IMessageQueue.QueueMessageAsync(IBotToUser botToUser, IMessageActivity message, CancellationToken token)
        {
            // This assumes that if InputHint is set on message, it is the right value that channel expects
            // and will NOT queue the message
            if (this.channelCapability.ShouldSetInputHint(message))
            {
                // drain the queue
                while (this.queue.Count > 0)
                {
                    var toUser = this.queue.Dequeue();
                    toUser.InputHint = InputHints.IgnoringInput;
                    await botToUser.PostAsync(toUser, token);
                }
                queue.Enqueue(message);
            }
            else
            {
                await botToUser.PostAsync(message, token);
            }
        }

        async Task IMessageQueue.DrainQueueAsync(IBotToUser botToUser, CancellationToken token)
        {
            while (this.queue.Count > 0)
            {
                var toUser = this.queue.Dequeue();
                // last message in the queue will be treated specially for channels that need input hints
                if (this.queue.Count == 0)
                {
                    var stack = this.makeStack();
                    if (this.channelCapability.ShouldSetInputHint(toUser) && stack.Frames.Count > 0)
                    {
                        var topOfStack = stack.Frames[0].Target;
                        // if there is a prompt dialog on top of stack, the InputHint will be set to Expecting
                        if (topOfStack != null && topOfStack.GetType().DeclaringType == typeof(PromptDialog))
                        {
                            toUser.InputHint = InputHints.ExpectingInput;
                        }
                        else
                        {
                            toUser.InputHint = InputHints.AcceptingInput;
                        }

                    }
                }
                else
                {

                    if (this.channelCapability.ShouldSetInputHint(toUser))
                    {
                        toUser.InputHint = InputHints.IgnoringInput;
                    }
                }

                await botToUser.PostAsync(toUser, token);
            }
        }
    }

    public interface IMessageActivityMapper
    {
        IMessageActivity Map(IMessageActivity message);
    }

#pragma warning disable CS0618
    public sealed class KeyboardCardMapper : IMessageActivityMapper
    {
        public IMessageActivity Map(IMessageActivity message)
        {
            if (message.Attachments.Any())
            {
                var keyboards = message.Attachments.Where(t => t.ContentType == KeyboardCard.ContentType).ToList();
                if (keyboards.Count > 1)
                {
                    throw new ArgumentException("Each message can only have one keyboard card!");
                }

                var keyboard = keyboards.FirstOrDefault();
                if (keyboard != null)
                {
                    message.Attachments.Remove(keyboard);
                    var keyboardCard = (KeyboardCard)keyboard.Content;
                    if (message.ChannelId == "facebook" && keyboardCard.Buttons.Count <= 10)
                    {
                        message.ChannelData = keyboardCard.ToFacebookMessage();
                    }
                    else
                    {
                        message.Attachments.Add(keyboardCard.ToHeroCard().ToAttachment());
                    }
                }
            }

            return message;
        }
    }

#pragma warning restore CS0618

    public sealed class SetLocalTimestampMapper : IMessageActivityMapper
    {
        public IMessageActivity Map(IMessageActivity message)
        {
            if (message.LocalTimestamp == null)
            {
                message.LocalTimestamp = DateTimeOffset.UtcNow;
            }
            return message;
        }
    }

    public sealed class MapToChannelData_BotToUser : IBotToUser
    {
        private readonly IBotToUser inner;
        private readonly IEnumerable<IMessageActivityMapper> mappers;

        public MapToChannelData_BotToUser(IBotToUser inner, IEnumerable<IMessageActivityMapper> mappers)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.mappers, nameof(mappers), mappers);
        }

        public async Task PostAsync(IMessageActivity message, CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var mapper in mappers)
            {
                message = mapper.Map(message);
            }
            await this.inner.PostAsync(message, cancellationToken);
        }

        public IMessageActivity MakeMessage()
        {
            return this.inner.MakeMessage();
        }
    }


    public sealed class BotToUserTextWriter : IBotToUser
    {
        private readonly IBotToUser inner;
        private readonly TextWriter writer;
        public BotToUserTextWriter(IBotToUser inner, TextWriter writer)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.writer, nameof(writer), writer);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            return this.inner.MakeMessage();
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.inner.PostAsync(message, cancellationToken);
            await this.writer.WriteLineAsync($"{message.Text}{ButtonsToText(message.Attachments)}");
        }

        private static string ButtonsToText(IList<Attachment> attachments)
        {
            var cardAttachments = attachments?.Where(attachment => attachment.ContentType.StartsWith("application/vnd.microsoft.card"));
            var builder = new StringBuilder();
            if (cardAttachments != null && cardAttachments.Any())
            {
                builder.AppendLine();
                foreach (var attachment in cardAttachments)
                {
                    string type = attachment.ContentType.Split('.').Last();
                    if (type == "hero" || type == "thumbnail")
                    {
                        var card = (HeroCard)attachment.Content;
                        if (!string.IsNullOrEmpty(card.Title))
                        {
                            builder.AppendLine(card.Title);
                        }
                        if (!string.IsNullOrEmpty(card.Subtitle))
                        {
                            builder.AppendLine(card.Subtitle);
                        }
                        if (!string.IsNullOrEmpty(card.Text))
                        {
                            builder.AppendLine(card.Text);
                        }
                        if (card.Buttons != null)
                        {
                            foreach (var button in card.Buttons)
                            {
                                builder.AppendLine($"* {button.Title}");
                            }
                        }
                    }
                }
            }
            return builder.ToString();
        }
    }
}
