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
    public class BotContext : IBotContext
    {
        private readonly BotAdapter _adapter;
        private readonly Activity _request;
        private readonly ConversationReference _conversationReference;
        private IList<Activity> _responses = new List<Activity>();
        private Dictionary<string, object> _services = new Dictionary<string, object>();

        public BotContext(BotAdapter adapter, Activity request)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
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

        public BotContext(BotAdapter adapter, ConversationReference conversationReference)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _conversationReference = conversationReference ?? throw new ArgumentNullException(nameof(conversationReference));
        }
        public BotAdapter Adapter => _adapter;

        public Activity Request => _request;

        public IList<Activity> Responses { get => _responses; set => this._responses = value; }

        public ConversationReference ConversationReference { get => _conversationReference; }

        public IBotContext Reply(string text, string speak = null)
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

        public IBotContext Reply(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);
            this.Responses.Add((Activity)activity);
            return this;
        }

        public void Set(string objectId, object service)
        {
            if (String.IsNullOrWhiteSpace(objectId))
                throw new ArgumentNullException(nameof(objectId));
            lock (_services)
            {
                this._services[objectId] = service;
            }
        }

        public object Get(string objectId)
        {
            if (String.IsNullOrWhiteSpace(objectId))
                throw new ArgumentNullException(nameof(objectId));
            object service = null;
            lock (_services)
            {
                this._services.TryGetValue(objectId, out service);
            }
            return service;
        }
    }

}
