// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Defines a state management object and automates the reading and writing of associated state
    /// properties to a storage layer.
    /// </summary>
    /// <remarks>
    /// Each state management object defines a scope for a storage layer.
    ///
    /// State properties are created within a state management scope, and the Bot Framework
    /// defines these scopes:
    /// <see cref="ConversationState"/>, <see cref="UserState"/>, and <see cref="PrivateConversationState"/>.
    ///
    /// You can define additional scopes for your bot.
    /// </remarks>
    /// <seealso cref="IStorage"/>
    /// <seealso cref="IStatePropertyAccessor{T}"/>
    public abstract class BotState : IPropertyManager
    {
        private readonly string _contextServiceKey;
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotState"/> class.
        /// </summary>
        /// <param name="storage">The storage layer this state management object will use to store
        /// and retrieve state.</param>
        /// <param name="contextServiceKey">The key for the state cache for this <see cref="BotState"/>.</param>
        /// <remarks>This constructor creates a state management object and associated scope.
        /// The object uses <paramref name="storage"/> to persist state property values.
        /// The object uses the <paramref name="contextServiceKey"/> to cache state within the context for each turn.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="storage"/> or <paramref name="contextServiceKey"/>
        /// is <c>null</c>.</exception>
        /// <seealso cref="ITurnContext"/>
        public BotState(IStorage storage, string contextServiceKey)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _contextServiceKey = contextServiceKey ?? throw new ArgumentNullException(nameof(contextServiceKey));
        }

        /// <summary>
        /// Creates a named state property within the scope of a <see cref="BotState"/> and returns
        /// an accessor for the property.
        /// </summary>
        /// <typeparam name="T">The value type of the property.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <returns>An accessor for the property.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
        public IStatePropertyAccessor<T> CreateProperty<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new BotStatePropertyAccessor<T>(this, name);
        }

        /// <summary>
        /// Populates the state cache for this <see cref="BotState"/> from the storage layer.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="force">Optional, <c>true</c> to overwrite any existing state cache;
        /// or <c>false</c> to load state from storage only if the cache doesn't already exist.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public virtual async Task LoadAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var cachedState = turnContext.TurnState.Get<CachedBotState>(_contextServiceKey);
            var storageKey = GetStorageKey(turnContext);
            if (force || cachedState == null || cachedState.State == null)
            {
                var items = await _storage.ReadAsync(new[] { storageKey }, cancellationToken).ConfigureAwait(false);
                items.TryGetValue(storageKey, out object val);
                turnContext.TurnState[_contextServiceKey] = new CachedBotState((IDictionary<string, object>)val);
            }
        }

        /// <summary>
        /// Writes the state cache for this <see cref="BotState"/> to the storage layer.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="force">Optional, <c>true</c> to save the state cache to storage;
        /// or <c>false</c> to save state to storage only if a property in the cache has changed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public virtual async Task SaveChangesAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var cachedState = turnContext.TurnState.Get<CachedBotState>(_contextServiceKey);
            if (cachedState != null && (force || cachedState.IsChanged()))
            {
                var key = GetStorageKey(turnContext);
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
        /// Clears the state cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This method clears the state cache in the turn context. Call
        /// <see cref="SaveChangesAsync(ITurnContext, bool, CancellationToken)"/> to persist this
        /// change in the storage layer.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public virtual Task ClearStateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Explicitly setting the hash will mean IsChanged is always true. And that will force a Save.
            turnContext.TurnState[_contextServiceKey] = new CachedBotState { Hash = string.Empty };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes any state in storage and the cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public virtual async Task DeleteAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var cachedState = turnContext.TurnState.Get<CachedBotState>(_contextServiceKey);
            if (cachedState != null)
            {
                turnContext.TurnState.Remove(_contextServiceKey);
            }

            var storageKey = GetStorageKey(turnContext);
            await _storage.DeleteAsync(new[] { storageKey }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a copy of the raw cached data for this <see cref="BotState"/> from the turn context.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>A JSON representation of the cached state.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public JToken Get(ITurnContext turnContext)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var stateKey = this.GetType().Name;
            var cachedState = turnContext.TurnState.Get<object>(stateKey);
            return JObject.FromObject(cachedState)["State"];
        }

        /// <summary>
        /// When overridden in a derived class, gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        protected abstract string GetStorageKey(ITurnContext turnContext);

        /// <summary>
        /// Gets the value of a property from the state cache for this <see cref="BotState"/>.
        /// </summary>
        /// <typeparam name="T">The value type of the property.</typeparam>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains the property value.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> or
        /// <paramref name="propertyName"/> is <c>null</c>.</exception>
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

            var cachedState = turnContext.TurnState.Get<CachedBotState>(_contextServiceKey);

            // if there is no value, this will throw, to signal to IPropertyAccesor that a default value should be computed
            // This allows this to work with value types
            return Task.FromResult((T)cachedState.State[propertyName]);
        }

        /// <summary>
        /// Deletes a property from the state cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> or
        /// <paramref name="propertyName"/> is <c>null</c>.</exception>
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

            var cachedState = turnContext.TurnState.Get<CachedBotState>(_contextServiceKey);
            cachedState.State.Remove(propertyName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the value of a property in the state cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The value to set on the property.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> or
        /// <paramref name="propertyName"/> is <c>null</c>.</exception>
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

            var cachedState = turnContext.TurnState.Get<CachedBotState>(_contextServiceKey);
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
                State = state ?? new ConcurrentDictionary<string, object>();
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
        /// Note the semantic of this accessor are intended to be lazy, this means teh Get, Set and Delete
        /// methods will first call LoadAsync. This will be a no-op if the data is already loaded.
        /// The implication is you can just use this accessor in the application code directly without first calling LoadAsync
        /// this approach works with the AutoSaveStateMiddleware which will save as needed at the end of a turn.
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
            /// Delete the property. The semantics are intended to be lazy, note the use of LoadAsync at the start.
            /// </summary>
            /// <param name="turnContext">The turn context.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public async Task DeleteAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                await _botState.LoadAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
                await _botState.DeletePropertyValueAsync(turnContext, Name, cancellationToken).ConfigureAwait(false);
            }

            /// <summary>
            /// Get the property value. The semantics are intended to be lazy, note the use of LoadAsync at the start.
            /// </summary>
            /// <param name="turnContext">The context object for this turn.</param>
            /// <param name="defaultValueFactory">Defines the default value. Invoked when no value been set for the requested state property.  If defaultValueFactory is defined as null, the MissingMemberException will be thrown if the underlying property is not set.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public async Task<T> GetAsync(ITurnContext turnContext, Func<T> defaultValueFactory, CancellationToken cancellationToken)
            {
                await _botState.LoadAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
                try
                {
                    return await _botState.GetPropertyValueAsync<T>(turnContext, Name, cancellationToken).ConfigureAwait(false);
                }
                catch (KeyNotFoundException)
                {
                    // ask for default value from factory
                    if (defaultValueFactory == null)
                    {
                        return default(T);
                    }

                    var result = defaultValueFactory();

                    // save default value for any further calls
                    await SetAsync(turnContext, result, cancellationToken).ConfigureAwait(false);
                    return result;
                }
            }

            /// <summary>
            /// Set the property value. The semantics are intended to be lazy, note the use of LoadAsync at the start.
            /// </summary>
            /// <param name="turnContext">turn context.</param>
            /// <param name="value">value.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public async Task SetAsync(ITurnContext turnContext, T value, CancellationToken cancellationToken)
            {
                await _botState.LoadAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
                await _botState.SetPropertyValueAsync(turnContext, Name, value, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
