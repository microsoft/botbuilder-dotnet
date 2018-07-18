// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Reads and writes state for your bot to storage.
    /// </summary>
    /// <remarks>
    /// The state object will be automatically cached on the context object for the lifetime of the turn
    /// and will only be written to storage if it has been modified.
    ///
    /// When a `BotState` instance is used as middleware its state object will be automatically read in
    /// before your bots logic runs and then intelligently written back out upon completion of your bots
    /// logic. Multiple instances can be read and written in parallel using the `BotStateSet` middleware.
    ///
    /// ```JavaScript
    /// const { BotState, MemoryStorage
    /// } require('botbuilder');
    ///
    /// const storage = new MemoryStorage();
    /// const botState = new BotState(storage, (context) => 'botState');
    /// adapter.use(botState);
    ///
    /// server.post('/api/messages', (req, res) => {
    ///  *adapter.processActivity(req, res, async (context) => {
    ///  *       // Track up time
    ///  *       const state = botState.get(context);
    ///  *       if (!('startTime' in state)) { state.startTime = new Date().getTime() }
    ///  *state.upTime = new Date().getTime() - state.stateTime;
    ///  *
    ///  *       // ... route activity ...
    ///  *
    ///  *    });
    /// });
    /// ```
    /// </remarks>
    /// <typeparam name="TState">The type of the bot state object.</typeparam>
    public class BotState : IMiddleware
    {
        private string _contextServiceKey;
        private readonly IStorage _storage;
        private readonly Func<ITurnContext, string> _storageKeyDelegate;
        private readonly Dictionary<string, IPropertyAccessor> _properties = new Dictionary<string, IPropertyAccessor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BotState"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="contextServiceKey">the key for caching on the context services dictionary</param>
        /// <param name="storageKeyDelegate">A function that can provide the key to persistent storage.</param>
        public BotState(IStorage storage, string contextServiceKey, Func<ITurnContext, string> storageKeyDelegate)
        {
            this._storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this._contextServiceKey = contextServiceKey ?? throw new ArgumentNullException(nameof(contextServiceKey));
            this._storageKeyDelegate = storageKeyDelegate ?? throw new ArgumentNullException(nameof(storageKeyDelegate));
        }

        /// <summary>
        /// Create a property definition and register it with this BotState.
        /// </summary>
        /// <typeparam name="T">type of property</typeparam>
        /// <param name="name">name of the property</param>
        /// <param name="defaultValueFactory">default value</param>
        /// <returns>returns an IPropertyAccessor</returns>
        public IPropertyAccessor<T> CreateProperty<T>(string name, Func<T> defaultValueFactory = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var prop = new BotStatePropertyAccessor<T>(this, name, defaultValueFactory);
            this._properties.Add(name, prop);
            return prop;
        }

        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This middleware loads the state object on the leading edge of the middleware pipeline
        /// and persists the state object on the trailing edge.
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
            await SaveChangesAsync(context, cancellationToken: cancellationToken).ConfigureAwait(false);
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

            var cachedState = context.Services.Get<CachedBotState>(this._contextServiceKey);
            var storageKey = _storageKeyDelegate(context);
            if (force || cachedState == null || cachedState.State == null)
            {
                var items = await _storage.ReadAsync(new[] { storageKey }, cancellationToken).ConfigureAwait(false);
                items.TryGetValue(storageKey, out object val);
                context.Services[_contextServiceKey] = new CachedBotState((IDictionary<string, object>)val ?? new Dictionary<string, object>());
            }
        }

        /// <summary>
        /// Writes the state object cached in the TurnContext if it is changed
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

            var cachedState = context.Services.Get<CachedBotState>(this._contextServiceKey);
            if (force || cachedState.IsChanged())
            {
                var key = _storageKeyDelegate(context);
                var changes = new Dictionary<string, object>();
                changes.Add(key, cachedState.State);
                await this._storage.WriteAsync(changes).ConfigureAwait(false);
                context.Services[this._contextServiceKey] = null;
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

            var cachedState = context.Services.Get<CachedBotState>(this._contextServiceKey);
            if (cachedState != null)
            {
                context.Services[this._contextServiceKey] = new CachedBotState();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// IPropertyContainer.GetPropertyAasync().
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="propertyName">name of the property</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>T</returns>
        public Task<T> GetPropertyValueAsync<T>(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.Services.Get<CachedBotState>(this._contextServiceKey);

            // if there is no value, this will throw, to signal to IPropertyAccesor that a default value should be computed
            // This allows this to work with value types
            return Task.FromResult((T)cachedState.State[propertyName]);
        }

        /// <summary>
        /// IPropertyContainer.DeleteAsync() method gives IPropertyAccessor ability to delete from it's container.
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="propertyName">name of the property</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public Task DeletePropertyValueAsync(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.Services.Get<CachedBotState>(this._contextServiceKey);
            cachedState.State.Remove(propertyName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// IPropertyContainer.SetAsync() method gives IPropertyAccessor ability to set the value in it's container.
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="propertyName">name of the property</param>
        /// <param name="value">value of the property</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Task</returns>
        public Task SetPropertyValueAsync(ITurnContext turnContext, string propertyName, object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.Services.Get<CachedBotState>(this._contextServiceKey);
            cachedState.State[propertyName] = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Internal cached bot state.
        /// </summary>
        internal class CachedBotState
        {
            internal CachedBotState(IDictionary<string, object> state = null)
            {
                this.State = state ?? new Dictionary<string, object>();
                this.Hash = ComputeHash(this.State);
            }

            internal IDictionary<string, object> State { get; set; }

            internal string Hash { get; set; }

            internal bool IsChanged()
            {
                return Hash != ComputeHash(this.State);
            }

            private string ComputeHash(object obj)
            {
                return JsonConvert.SerializeObject(obj);
            }
        }

        /// <summary>
        /// Implements IPropertyAccessor for an IPropertyContainer.
        /// </summary>
        /// <typeparam name="T">type of value the propertyAccessor accesses.</typeparam>
        internal class BotStatePropertyAccessor<T> : IPropertyAccessor<T>
        {
            private BotState _botState;
            private Func<T> _defaultValueFactory;

            public BotStatePropertyAccessor(BotState botState, string name, Func<T> defaultValueFactory)
            {
                this._botState = botState;
                this.Name = name;
                if (defaultValueFactory == null)
                {
                    this._defaultValueFactory = () => default(T);
                }
                else
                {
                    this._defaultValueFactory = defaultValueFactory;
                }
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
                return this._botState.DeletePropertyValueAsync(turnContext, this.Name);
            }

            /// <summary>
            /// Get the property value.
            /// </summary>
            /// <param name="turnContext">turn context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public async Task<T> GetAsync(ITurnContext turnContext)
            {
                try
                {
                    return await this._botState.GetPropertyValueAsync<T>(turnContext, this.Name).ConfigureAwait(false);
                }
                catch (KeyNotFoundException)
                {
                    // ask for default value from factory
                    var result = _defaultValueFactory();

                    // save default value for any further calls
                    await this.SetAsync(turnContext, result).ConfigureAwait(false);
                    return result;
                }
            }

            /// <summary>
            /// Set the property value.
            /// </summary>
            /// <param name="turnContext">turn context.</param>
            /// <param name="value">value.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public Task SetAsync(ITurnContext turnContext, T value)
            {
                return this._botState.SetPropertyValueAsync(turnContext, this.Name, value);
            }
        }
    }
}
