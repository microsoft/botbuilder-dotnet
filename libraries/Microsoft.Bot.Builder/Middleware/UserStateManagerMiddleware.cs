// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Middleware
{
    public class UserState : StoreItem
    {
    }

    public class UserStateManagerSettings
    {        
        public bool PersistUserState { get; set; } = true;
        public bool WriteBeforeSend { get; set; } = true;
        public bool LastWriterWins { get; set; } = true; 
    }

    public class UserStateManagerMiddleware : IContextCreated, ISendActivity
    {
        private readonly UserStateManagerSettings _settings;
        private readonly IStorage _storage;
        private const string UserKeyRoot = @"user";

        public UserStateManagerMiddleware(IStorage storage) : this(storage, new UserStateManagerSettings())
        {
        }

        public UserStateManagerMiddleware(IStorage storage, UserStateManagerSettings settings)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _settings = settings ?? throw new ArgumentNullException("settings");
        }

        public async Task ContextCreated(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            await Read(context).ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }

        public async Task SendActivity(IBotContext context, IList<IActivity> activities, MiddlewareSet.NextDelegate next)
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

            if (_settings.PersistUserState)
                keys.Add(UserKey(context));

            var items = await _storage.Read(keys.ToArray());

            string userKey = UserKey(context);        

            context.State.User = items.Get<UserState>(userKey) ?? new UserState();            

            return items;
        }

        protected virtual async Task Write(IBotContext context, StoreItems changes = null)
        {
            if (changes == null)
                changes = new StoreItems();

            if (this._settings.PersistUserState)
            {
                changes[UserKey(context)] = context.State.User ?? new UserState();
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
                if (key.StartsWith(UserKeyRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Keys starting with '{UserKeyRoot}' are reserved.");
            }
        }

        private static string UserKey(IBotContext context)
        {
            var conversation = context.ConversationReference;
            return $"{UserKeyRoot}/{conversation.ChannelId}/{conversation.User.Id}";
        }
    }
}
