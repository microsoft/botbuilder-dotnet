// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Builder
{
    public class BotContext : FlexObject, IBotContext
    {
        private readonly Bot _bot;
        private readonly IActivity _request;
        private readonly ConversationReference _conversationReference;
        private readonly BotState _state = new BotState();
        private IList<IActivity> _responses = new List<IActivity>();

        public BotContext(Bot bot, IActivity request)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _request = request ?? throw new ArgumentNullException(nameof(request));

            _conversationReference = new ConversationReference()
            {
                ActivityId = request.Id,
                User = request.From,
                Bot = request.Recipient,
                Conversation = request.Conversation,
                ChannelId = request.ChannelId,
                ServiceUrl = request.ServiceUrl
            };
        }

        public BotContext(Bot bot, ConversationReference conversationReference)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _conversationReference = conversationReference ?? throw new ArgumentNullException(nameof(conversationReference));
        }

        public async Task PostActivity(IBotContext context, IList<IActivity> activities)
        {
            await _bot.PostActivity(context, activities).ConfigureAwait(false);
        }

        public IActivity Request => _request;

        public Bot Bot => _bot;

        public IList<IActivity> Responses { get => _responses; set => this._responses = value; }

        public IStorage Storage { get; set; }

        public Intent TopIntent { get; set; }

        public TemplateManager TemplateManager { get; set; }

        public bool IfIntent(string intentName)
        {
            if (string.IsNullOrWhiteSpace(intentName))
                throw new ArgumentNullException(nameof(intentName));

            if (this.TopIntent != null)
            {
                if (TopIntent.Name == intentName)
                {
                    return true;
                }
            }

            return false;
        }
        public bool IfIntent(Regex expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (this.TopIntent != null)
            {
                if (expression.IsMatch(this.TopIntent.Name))
                    return true;
            }

            return false;
        }


        public ConversationReference ConversationReference { get => _conversationReference; }

        public BotState State { get => _state; }

        public BotContext Reply(string text, string speak = null)
        {
            var reply = this.ConversationReference.GetPostToUserMessage();
            reply.Text = text;
            if (!string.IsNullOrWhiteSpace(speak))
            {
                // Developer included SSML to attach to the message.
                reply.Speak = speak;

            }
            this.Responses.Add(reply);
            return this;
        }

        public BotContext Reply(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);
            this.Responses.Add(activity);
            return this;
        }
        
        public BotContext ReplyWith(string templateId, object data = null)
        {
            // queue template activity to be databound when sent
            var reply = this.ConversationReference.GetPostToUserMessage();
            reply.Type = TemplateManager.TEMPLATE;
            reply.Text = templateId;
            reply.Value = data;
            this.Responses.Add(reply);
            return this;
        }
    }
}
