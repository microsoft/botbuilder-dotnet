// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Reads and writes state for your bot to storage.
    /// </summary>
    public abstract class BotState : IMiddleware
    {
        private readonly string _contextServiceKey;
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotState"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="contextServiceKey">the key for caching on the context services dictionary.</param>
        public BotState(IStorage storage, string contextServiceKey)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _contextServiceKey = contextServiceKey ?? throw new ArgumentNullException(nameof(contextServiceKey));
        }

        /// <summary>
        /// Create a property definition and register it with this BotState.
        /// </summary>
        /// <typeparam name="T">type of property.</typeparam>
        /// <param name="name">name of the property.</param>
        /// <returns>returns an IPropertyAccessor</returns>
        public IStatePropertyAccessor<T> CreateProperty<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new BotStatePropertyAccessor<T>(this, name);
        }

        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This middleware loads the state object on the leading edge of the middleware pipeline
        /// and persists the state object on the trailing edge. Note this is different than BotStateSet,
        /// which does not pre-load the set on entry into the pipeline.
        /// </remarks>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            // Load state
            await LoadAsync(context, true, cancellationToken).ConfigureAwait(false);

            // process activity
            await next(cancellationToken).ConfigureAwait(false);

            // Save changes
            await SaveChangesAsync(context, false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads in and caches the current state object in the TurnContext
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="force">(optional) if true the cache will be bypassed </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If successful, the task result contains the state object, read from storage.</remarks>
        public async Task LoadAsync(ITurnContext context, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cachedState = context.Services.Get<CachedBotState>(_contextServiceKey);
            var storageKey = GetStorageKey(context);
            if (force || cachedState == null || cachedState.State == null)
            {
                var items = await _storage.ReadAsync(new[] { storageKey }, cancellationToken).ConfigureAwait(false);
                items.TryGetValue(storageKey, out object val);
                context.Services[_contextServiceKey] = new CachedBotState((IDictionary<string, object>)val ?? new Dictionary<string, object>());
            }
        }

        /// <summary>
        /// Writes the state object cached in the TurnContext if it is changed.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="force">force the saving of changes even if there are no changes.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SaveChangesAsync(ITurnContext context, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cachedState = context.Services.Get<CachedBotState>(_contextServiceKey);
            if (force || (cachedState != null && cachedState.IsChanged()))
            {
                var key = GetStorageKey(context);
                var changes = new Dictionary<string, object>
                {
                    { key, cachedState.State },
                };
                await _storage.WriteAsync(changes).ConfigureAwait(false);
                cachedState.Hash = cachedState.ComputeHash(cachedState.State);
                return;
            }
        }

        /// <summary>
        /// Reset the state object to it's default form.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="cancellationToken">cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task ClearStateAsync(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cachedState = context.Services.Get<CachedBotState>(_contextServiceKey);
            if (cachedState != null)
            {
                context.Services[_contextServiceKey] = new CachedBotState();
            }

            return Task.CompletedTask;
        }

        protected abstract string GetStorageKey(ITurnContext context);

        /// <summary>
        /// gives IPropertyAccessor ability to get property Value from container.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="propertyName">name of the property.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>T</returns>
        protected Task<T> GetPropertyValueAsync<T>(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.Services.Get<CachedBotState>(_contextServiceKey);

            // if there is no value, this will throw, to signal to IPropertyAccesor that a default value should be computed
            // This allows this to work with value types
            return Task.FromResult((T)cachedState.State[propertyName]);
        }

        /// <summary>
        /// gives IPropertyAccessor ability to delete from it's container.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="propertyName">name of the property.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>Task</returns>
        protected Task DeletePropertyValueAsync(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.Services.Get<CachedBotState>(_contextServiceKey);
            cachedState.State.Remove(propertyName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// gives IPropertyAccessor ability to set the value in it's container.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="propertyName">name of the property.</param>
        /// <param name="value">value of the property.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>Task</returns>
        protected Task SetPropertyValueAsync(ITurnContext turnContext, string propertyName, object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.Services.Get<CachedBotState>(_contextServiceKey);
            cachedState.State[propertyName] = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Internal cached bot state.
        /// </summary>
        private class CachedBotState
        {
            public CachedBotState(IDictionary<string, object> state = null)
            {
                State = state ?? new Dictionary<string, object>();
                Hash = ComputeHash(State);
            }

            public IDictionary<string, object> State { get; set; }

            public string Hash { get; set; }

            public bool IsChanged()
            {
                return Hash != ComputeHash(State);
            }

            internal string ComputeHash(object obj)
            {
                return JsonConvert.SerializeObject(obj);
            }
        }

        /// <summary>
        /// Implements IPropertyAccessor for an IPropertyContainer.
        /// </summary>
        /// <typeparam name="T">type of value the propertyAccessor accesses.</typeparam>
        private class BotStatePropertyAccessor<T> : IStatePropertyAccessor<T>
        {
            private BotState _botState;

            public BotStatePropertyAccessor(BotState botState, string name)
            {
                _botState = botState;
                Name = name;
            }

            /// <summary>
            /// Gets name of the property.
            /// </summary>
            /// <value>
            /// name of the property.
            /// </value>
            public string Name { get; private set; }

            /// <summary>
            /// Delete the property.
            /// </summary>
            /// <param name="turnContext">turn context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public Task DeleteAsync(ITurnContext turnContext)
            {
                return _botState.DeletePropertyValueAsync(turnContext, Name);
            }

            /// <summary>
            /// Get the property value.
            /// </summary>
            /// <param name="turnContext">The context object for this turn.</param>
            /// <param name="defaultValueFactory">Defines the default value. Invoked when no value been set for the requested state property.  If defaultValueFactory is defined as null, the MissingMemberException will be thrown if the underlying property is not set.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public async Task<T> GetAsync(ITurnContext turnContext, Func<T> defaultValueFactory = null)
            {
                await _botState.LoadAsync(turnContext).ConfigureAwait(false);
                try
                {
                    return await _botState.GetPropertyValueAsync<T>(turnContext, Name).ConfigureAwait(false);
                }
                catch (KeyNotFoundException)
                {
                    // ask for default value from factory
                    if (defaultValueFactory == null)
                    {
                        throw new MissingMemberException("Property not set and no default provided.");
                    }

                    var result = defaultValueFactory();

                    // save default value for any further calls
                    await SetAsync(turnContext, result).ConfigureAwait(false);
                    return result;
                }
            }

            /// <summary>
            /// Set the property value.
            /// </summary>
            /// <param name="turnContext">turn context.</param>
            /// <param name="value">value.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public async Task SetAsync(ITurnContext turnContext, T value)
            {
                await _botState.LoadAsync(turnContext).ConfigureAwait(false);
                await _botState.SetPropertyValueAsync(turnContext, Name, value).ConfigureAwait(false);
            }
        }
    }
}
