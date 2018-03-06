// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Middleware
{
    public class StateSettings
    {
        public bool WriteBeforeSend { get; set; } = true;
        public bool LastWriterWins { get; set; } = true;
    }

    /// <summary>
    /// Abstract Base class which manages details of auto loading/saving of BotState
    /// </summary>
    /// <typeparam name="StateT"></typeparam>
    public abstract class BotState<StateT> : IContextCreated, ISendActivity
        where StateT : new()
    {
        private readonly StateSettings _settings;
        private readonly IStorage _storage;
        private readonly Func<IBotContext, string> _keyDelegate;
        private readonly string _propertyName;

        /// <summary>
        /// Create statemiddleware
        /// </summary>
        /// <param name="name">name of the kind of state</param>
        /// <param name="storage">storage provider to use</param>
        /// <param name="settings">settings</param>
        public BotState(IStorage storage, string propertyName, Func<IBotContext, string> keyDelegate, StateSettings settings = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _keyDelegate = keyDelegate ?? throw new ArgumentNullException(nameof(keyDelegate));
            _settings = settings ?? new StateSettings();
        }

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
            var key = this._keyDelegate(context);
            var keys = new List<String>();
            keys.Add(key);
            var items = await _storage.Read(keys.ToArray());
            var state = items.Get<StateT>(key);
            if (state == null)
                state = new StateT();
            context.Set(this._propertyName, state);
            return items;
        }

        protected virtual async Task Write(IBotContext context)
        {
            StoreItems changes = new StoreItems();

            var state = context.Get<StateT>(this._propertyName);
            if (state == null)
                state = new StateT();
            var key = _keyDelegate(context);
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

    /// <summary>
    /// Handles persistence of StateT object using Context.Request.Conversation.Id as the key
    /// </summary>
    /// <typeparam name="StateT"></typeparam>
    public class ConversationState<StateT> : BotState<StateT>
        where StateT : new()
    {
        public static string PropertyName = $"ConversationState:{typeof(ConversationState<StateT>).Namespace}.{typeof(ConversationState<StateT>).Name}";

        public ConversationState(IStorage storage, StateSettings settings = null) :
            base(storage, PropertyName,
                (context) => $"conversation/{context.ConversationReference.ChannelId}/{context.ConversationReference.Conversation.Id}",
                settings)
        {
        }

        /// <summary>
        /// get the value of the ConversationState from the context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static StateT Get(IBotContext context) { return context.Get<StateT>(PropertyName); }
    }

    /// <summary>
    /// Handles persistence of StateT object using Context.Request.From.Id (aka user id) as the key
    /// </summary>
    /// <typeparam name="StateT"></typeparam>
    public class UserState<StateT> : BotState<StateT>
        where StateT : new()
    {
        public static readonly string PropertyName = $"UserState:{typeof(UserState<StateT>).Namespace}.{typeof(UserState<StateT>).Name}";

        public UserState(IStorage storage, StateSettings settings = null) :
            base(storage,
                PropertyName,
                (context) => $"user/{context.ConversationReference.ChannelId}/{context.ConversationReference.User.Id}")
        {
        }

        /// <summary>
        /// get the value of the ConversationState from the context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static StateT Get(IBotContext context) { return context.Get<StateT>(PropertyName); }
    }

    public static class StateContextExtensions
    {
        public static T GetConversationState<T>(this IBotContext context)
            where T : new()
        {
            return ConversationState<T>.Get(context);
        }

        public static T GetUserState<T>(this IBotContext context)
            where T : new()
        {
            return UserState<T>.Get(context);
        }
    }
}
