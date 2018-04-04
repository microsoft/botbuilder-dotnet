// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
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
    public abstract class BotState<StateT> : IMiddleware
        where StateT : class, new()
    {
        private readonly StateSettings _settings;
        private readonly IStorage _storage;
        private readonly Func<ITurnContext, string> _keyDelegate;
        private readonly string _propertyName;

        /// <summary>
        /// Create statemiddleware
        /// </summary>
        /// <param name="name">name of the kind of state</param>
        /// <param name="storage">storage provider to use</param>
        /// <param name="settings">settings</param>
        public BotState(IStorage storage, string propertyName, Func<ITurnContext, string> keyDelegate, StateSettings settings = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _keyDelegate = keyDelegate ?? throw new ArgumentNullException(nameof(keyDelegate));
            _settings = settings ?? new StateSettings();
        }

        public async Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            await Read(context).ConfigureAwait(false);
            await next().ConfigureAwait(false);
            await Write(context).ConfigureAwait(false);
        }

        protected virtual async Task<StoreItems> Read(ITurnContext context)
        {
            var key = this._keyDelegate(context);
            var keys = new List<String> { key };
            var items = await _storage.Read(keys.ToArray());
            var state = items.Get<StateT>(key);
            if (state == null)
                state = new StateT();
            context.Services.Add(this._propertyName, state);
            return items;
        }

        protected virtual async Task Write(ITurnContext context)
        {
            StoreItems changes = new StoreItems();

            var state = context.Services.Get<StateT>(this._propertyName);
            if (state == null)
                state = new StateT();
            var key = _keyDelegate(context);
            changes[key] = state;

            if (this._settings.LastWriterWins)
            {
                foreach (var item in changes)
                {
                    if(item.Value is IStoreItem valueStoreItem)
                    {
                        valueStoreItem.eTag = "*";
                    }
                }
            }

            await _storage.Write(changes).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles persistence of StateT object using Context.Activity.Conversation.Id as the key
    /// </summary>
    /// <typeparam name="StateT"></typeparam>
    public class ConversationState<StateT> : BotState<StateT>
        where StateT : class, new()
    {
        public static string PropertyName = $"ConversationState:{typeof(ConversationState<StateT>).Namespace}.{typeof(ConversationState<StateT>).Name}";

        public ConversationState(IStorage storage, StateSettings settings = null) :
            base(storage, PropertyName,
                (context) => $"conversation/{context.Activity.ChannelId}/{context.Activity.Conversation.Id}",
                settings)
        {
        }

        /// <summary>
        /// get the value of the ConversationState from the context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static StateT Get(ITurnContext context) { return context.Services.Get<StateT>(PropertyName); }
    }

    /// <summary>
    /// Handles persistence of StateT object using Context.Activity.From.Id (aka user id) as the key
    /// </summary>
    /// <typeparam name="StateT"></typeparam>
    public class UserState<StateT> : BotState<StateT>
        where StateT : class, new()
    {
        public static readonly string PropertyName = $"UserState:{typeof(UserState<StateT>).Namespace}.{typeof(UserState<StateT>).Name}";

        public UserState(IStorage storage, StateSettings settings = null) :
            base(storage,
                PropertyName,
                (context) => $"user/{context.Activity.ChannelId}/{context.Activity.From.Id}")
        {
        }

        /// <summary>
        /// get the value of the ConversationState from the context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static StateT Get(ITurnContext context) { return context.Services.Get<StateT>(PropertyName); }
    }

    public static class StateContextExtensions
    {
        public static T GetConversationState<T>(this ITurnContext context)
            where T : class, new()
        {
            return ConversationState<T>.Get(context);
        }

        public static T GetUserState<T>(this ITurnContext context)
            where T : class, new()
        {
            return UserState<T>.Get(context);
        }
    }
}
