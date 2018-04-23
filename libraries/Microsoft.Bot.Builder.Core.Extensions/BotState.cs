// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class StateSettings
    {
        public bool WriteBeforeSend { get; set; } = true;
        public bool LastWriterWins { get; set; } = true;
    }

    /// <summary>
    /// Abstract Base class which manages details of automatic loading and saving of bot state.
    /// </summary>
    /// <typeparam name="TState">The type of the bot state object.</typeparam>
    public class BotState<TState> : IMiddleware
        where TState : class, new()
    {
        private readonly StateSettings _settings;
        private readonly IStorage _storage;
        private readonly Func<ITurnContext, string> _keyDelegate;
        private readonly string _propertyName;

        /// <summary>
        /// Creates a new <see cref="BotState{TState}"/> middleware object.
        /// </summary>
        /// <param name="name">The name to use to load or save the state object.</param>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="settings">The state persistance options to use.</param>
        public BotState(IStorage storage, string propertyName, Func<ITurnContext, string> keyDelegate, StateSettings settings = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _keyDelegate = keyDelegate ?? throw new ArgumentNullException(nameof(keyDelegate));
            _settings = settings ?? new StateSettings();
        }

        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This middleware loads the state object on the leading edge of the middleware pipeline
        /// and persists the state object on the trailing edge.
        /// </remarks>
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            await ReadToContextService(context).ConfigureAwait(false);
            await next().ConfigureAwait(false);
            await WriteFromContextService(context).ConfigureAwait(false);
        }

        protected virtual async Task ReadToContextService(ITurnContext context)
        {
            var key = this._keyDelegate(context);
            var items = await _storage.Read(new[] { key });
            var state = items.Where(entry => entry.Key == key).Select(entry => entry.Value).OfType<TState>().FirstOrDefault();
            if (state == null)
                state = new TState();
            context.Services.Add(this._propertyName, state);
        }

        protected virtual async Task WriteFromContextService(ITurnContext context)
        {
            var state = context.Services.Get<TState>(this._propertyName);
            await Write(context, state);
        }

        /// <summary>
        /// Reads state from storage.
        /// </summary>
        /// <typeparam name="TState">The type of the bot state object.</typeparam>
        /// <param name="context">The context object for this turn.</param>
        public virtual async Task<TState> Read(ITurnContext context)
        {
            var key = this._keyDelegate(context);
            var items = await _storage.Read(new[] { key });
            var state = items.Where(entry => entry.Key == key).Select(entry => entry.Value).OfType<TState>().FirstOrDefault();
            if (state == null)
                state = new TState();
            return state;
        }

        /// <summary>
        /// Writes state to storage.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="state">The state object.</param>
        public virtual async Task Write(ITurnContext context, TState state)
        {
            var changes = new List<KeyValuePair<string, object>>();

            if (state == null)
                state = new TState();
            var key = _keyDelegate(context);
            changes.Add(new KeyValuePair<string, object>(key, state));

            if (this._settings.LastWriterWins)
            {
                foreach (var item in changes)
                {
                    if (item.Value is IStoreItem valueStoreItem)
                    {
                        valueStoreItem.eTag = "*";
                    }
                }
            }

            await _storage.Write(changes).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles persistence of a conversation state object using the conversation ID as part of the key.
    /// </summary>
    /// <typeparam name="TState">The type of the conversation state object.</typeparam>
    public class ConversationState<TState> : BotState<TState>
        where TState : class, new()
    {
        /// <summary>
        /// The key to use to read and write this conversation state object to storage.
        /// </summary>
        public static string PropertyName = $"ConversationState:{typeof(ConversationState<TState>).Namespace}.{typeof(ConversationState<TState>).Name}";

        /// <summary>
        /// Creates a new <see cref="ConversationState{TState}"/> object.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="settings">The state persistance options to use.</param>
        public ConversationState(IStorage storage, StateSettings settings = null) :
            base(storage, PropertyName,
                (context) => $"conversation/{context.Activity.ChannelId}/{context.Activity.Conversation.Id}",
                settings)
        {
        }

        /// <summary>
        /// Gets the conversation state object from turn context.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <returns>The coversation state object.</returns>
        public static TState Get(ITurnContext context) { return context.Services.Get<TState>(PropertyName); }
    }

    /// <summary>
    /// Handles persistence of a user state object using the user ID as part of the key.
    /// </summary>
    /// <typeparam name="TState">The type of the user state object.</typeparam>
    public class UserState<TState> : BotState<TState>
        where TState : class, new()
    {
        /// <summary>
        /// The key to use to read and write this conversation state object to storage.
        /// </summary>
        public static readonly string PropertyName = $"UserState:{typeof(UserState<TState>).Namespace}.{typeof(UserState<TState>).Name}";

        /// <summary>
        /// Creates a new <see cref="UserState{TState}"/> object.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="settings">The state persistance options to use.</param>
        public UserState(IStorage storage, StateSettings settings = null) :
            base(storage,
                PropertyName,
                (context) => $"user/{context.Activity.ChannelId}/{context.Activity.From.Id}")
        {
        }

        /// <summary>
        /// Gets the user state object from turn context.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <returns>The user state object.</returns>
        public static TState Get(ITurnContext context) { return context.Services.Get<TState>(PropertyName); }
    }

    /// <summary>
    /// Provides helper methods for getting state objects from the turn context.
    /// </summary>
    public static class StateTurnContextExtensions
    {
        /// <summary>
        /// Gets a conversation state object from the turn context.
        /// </summary>
        /// <typeparam name="TState">The type of the state object to get.</typeparam>
        /// <param name="context">The context object for this turn.</param>
        /// <returns>The state object.</returns>
        public static TState GetConversationState<TState>(this ITurnContext context)
            where TState : class, new()
        {
            return ConversationState<TState>.Get(context);
        }

        /// <summary>
        /// Gets a user state object from the turn context.
        /// </summary>
        /// <typeparam name="TState">The type of the state object to get.</typeparam>
        /// <param name="context">The context object for this turn.</param>
        /// <returns>The state object.</returns>
        public static TState GetUserState<TState>(this ITurnContext context)
            where TState : class, new()
        {
            return UserState<TState>.Get(context);
        }
    }
}
