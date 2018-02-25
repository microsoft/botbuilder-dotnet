// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Middleware
{
    public class StateSettings
    {
        public bool WriteBeforeSend { get; set; } = true;
        public bool LastWriterWins { get; set; } = true;
    }

    public abstract class StateMiddleware<StateT> : IContextCreated, ISendActivity
        where StateT : IStoreItem, new()
    {
        private readonly StateSettings _settings;
        private readonly IStorage _storage;

        /// <summary>
        /// Create statemiddleware
        /// </summary>
        /// <param name="name">name of the kind of state</param>
        /// <param name="storage">storage provider to use</param>
        /// <param name="settings">settings</param>
        public StateMiddleware(IStorage storage, StateSettings settings = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _settings = settings ?? new StateSettings();
        }

        public abstract string GetPropertyName();

        /// <summary>
        /// The key extracted from the activity which is used to store/retrieve the state
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public abstract string GetStorageKey(IBotContext context);

        public async Task ContextCreated(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            await Read(context).ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }

        public async Task SendActivity(IBotContext context, IList<Activity> activities, MiddlewareSet.NextDelegate next)
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

        protected virtual async Task<StoreItems> Read(IBotContext context)
        {
            var key = this.GetStorageKey(context);
            var keys = new List<String>();
            keys.Add(key);
            var items = await _storage.Read(keys.ToArray());
            var state = items.Get<StateT>(key);
            if (state == null)
                state = new StateT();
            context.Set(GetPropertyName(), state);
            return items;
        }

        protected virtual async Task Write(IBotContext context, StoreItems changes = null)
        {
            if (changes == null)
                changes = new StoreItems();

            var state = context.Get<StateT>(GetPropertyName());
            if (state == null)
                state = new StateT();
            var key = GetStorageKey(context);
            changes[key] = state;

            if (this._settings.LastWriterWins)
            {
                foreach (var item in changes)
                {
                    ((StoreItem)changes[item.Key]).eTag = "*";
                }
            }

            await _storage.Write(changes).ConfigureAwait(false);
        }

    }

    public class ConversationStateMiddleware<StateT> : StateMiddleware<StateT>
        where StateT : IStoreItem, new()
    {

        public ConversationStateMiddleware(IStorage storage, StateSettings settings = null) :
            base(storage, settings)
        {
        }

        public static readonly string PropertyName = $"{typeof(ConversationStateMiddleware<StateT>).Namespace}.{typeof(ConversationStateMiddleware<StateT>).Name}";

        public override string GetPropertyName()
        {
            return PropertyName;
        }

        public override string GetStorageKey(IBotContext context)
        {
            var conversation = context.ConversationReference;
            return $"conversation/{conversation.ChannelId}/{conversation.Conversation.Id}";
        }
    }

    public class UserStateMiddleware<StateT> : StateMiddleware<StateT>
        where StateT : IStoreItem, new()
    {

        public UserStateMiddleware(IStorage storage, StateSettings settings = null) :
            base(storage, settings)
        {
        }

        public static readonly string PropertyName = $"{typeof(UserStateMiddleware<StateT>).Namespace}.{typeof(UserStateMiddleware<StateT>).Name}";

        public override string GetPropertyName()
        {
            return PropertyName;
        }

        public override string GetStorageKey(IBotContext context)
        {
            var conversation = context.ConversationReference;
            return $"user/{conversation.ChannelId}/{conversation.User.Id}";
        }
    }

    public static class StateContextExtensions
    {
        public static T GetConversationState<T>(this IBotContext context)
            where T : IStoreItem, new()
        {
            return context.Get<T>(ConversationStateMiddleware<T>.PropertyName);
        }

        public static T GetConversationState<T>(this BotContextWrapper context)
            where T : IStoreItem, new()
        {
            return context.Get<T>(ConversationStateMiddleware<T>.PropertyName);
        }

        public static T GetUserState<T>(this IBotContext context)
            where T : IStoreItem, new()
        {
            return context.Get<T>(UserStateMiddleware<T>.PropertyName);
        }

        public static T GetUserState<T>(this BotContextWrapper context)
            where T : IStoreItem, new()
        {
            return context.Get<T>(UserStateMiddleware<T>.PropertyName);
        }
    }
}
