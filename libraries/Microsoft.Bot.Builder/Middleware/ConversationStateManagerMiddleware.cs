// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Middleware.MiddlewareSet;

namespace Microsoft.Bot.Builder.Middleware
{
    public class ConversationState : StoreItem
    {
    }

    public class ConversationStateManagerSettings
    {
        public bool PersistConversationState { get; set; } = true;
        public bool WriteBeforeSend { get; set; } = true;
        public bool LastWriterWins { get; set; } = true;
    }

    public class ConversationStateManagerMiddleware : IContextCreated, ISendActivity
    {
        private readonly ConversationStateManagerSettings _settings;
        private readonly IStorage _storage;        
        private const string ConversationKeyRoot = @"conversation";

        public ConversationStateManagerMiddleware(IStorage storage) : this(storage, new ConversationStateManagerSettings())
        {
        }

        public ConversationStateManagerMiddleware(IStorage storage, ConversationStateManagerSettings settings)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _settings = settings ?? throw new ArgumentNullException("settings");
        }

        public async Task ContextCreated(IBotContext context, NextDelegate next)
        {
            await Read(context).ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }

        public async Task SendActivity(IBotContext context, IList<IActivity> activities, NextDelegate next)
        {
            if (_settings.WriteBeforeSend)
            {
                await Write(context).ConfigureAwait(false);
            }
            await next().ConfigureAwait(false);
            if (!_settings.WriteBeforeSend)
            {
                await Write(context).ConfigureAwait(false);
            }
        }

        protected virtual async Task<StoreItems> Read(IBotContext context, IList<String> keys = null)
        {
            if (keys == null)
                keys = new List<String>();

            AssertValidKeys(keys);

            if (_settings.PersistConversationState)
                keys.Add(ConversationKey(context));

            var items = await _storage.Read(keys.ToArray());

            string conversationKey = ConversationKey(context);
            context.State.Conversation = items.Get<ConversationState>(conversationKey) ?? new ConversationState();

            return items;
        }

        protected virtual async Task Write(IBotContext context, StoreItems changes = null)
        {
            if (changes == null)
                changes = new StoreItems();

            if (this._settings.PersistConversationState)
            {
                changes[ConversationKey(context)] = context.State.Conversation ?? new ConversationState();
            }

            if (this._settings.LastWriterWins)
            {
                foreach (var item in changes)
                {
                    ((StoreItem)changes[item.Key]).eTag = "*";
                }
            }

            await _storage.Write(changes).ConfigureAwait(false);
        }

        private static void AssertValidKeys(IList<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            foreach (string key in keys)
            {
                if (key.StartsWith(ConversationKeyRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Keys starting with '{ConversationKeyRoot}' are reserved.");
            }
        }

        private static string ConversationKey(IBotContext context)
        {
            var conversation = context.ConversationReference;
            return $"{ConversationKeyRoot}/{conversation.ChannelId}/{conversation.Conversation.Id}";
        }
    }
}
