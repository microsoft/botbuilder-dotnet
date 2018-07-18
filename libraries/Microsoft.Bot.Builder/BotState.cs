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
    ///} require('botbuilder');
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
    public class BotState : IMiddleware, IPropertyContainer
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
            this._contextServiceKey = contextServiceKey;
            this._storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this._storageKeyDelegate = storageKeyDelegate ?? throw new ArgumentNullException(nameof(storageKeyDelegate));
        }

        /// <summary>
        /// Create a property definition and register it with this BotState
        /// </summary>
        /// <typeparam name="T">type of property</typeparam>
        /// <param name="name">name of the property</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>returns an IPropertyAccessor</returns>
        public IPropertyAccessor<T> CreateProperty<T>(string name, T defaultValue = default(T))
        {
            var prop = new SimplePropertyAccessor<T>(this, name, defaultValue);
            this._properties.Add(name, prop);
            return prop;
        }

        /// <summary>
        /// Create a property definition and register it with this BotState
        /// </summary>
        /// <typeparam name="T">type of property</typeparam>
        /// <param name="name">name of the property</param>
        /// <param name="defaultValueFactory">default value</param>
        /// <returns>returns an IPropertyAccessor</returns>
        public IPropertyAccessor<T> CreateProperty<T>(string name, Func<T> defaultValueFactory)
        {
            var prop = new SimplePropertyAccessor<T>(this, name, defaultValueFactory);
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
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            await LoadAsync(context, true, cancellationToken).ConfigureAwait(false);
            await next(cancellationToken).ConfigureAwait(false);
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
        /// <param name="state">The state object.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SaveChangesAsync(ITurnContext context, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cachedState = context.Services.Get<CachedBotState>(this._contextServiceKey);
            if (force || cachedState.IsChanged())
            {
                var key = _storageKeyDelegate(context);
                var changes = new Dictionary<string, object>();
                changes.Add(key, cachedState.State);
                await this._storage.WriteAsync(changes).ConfigureAwait(false);
                context.Services[this._contextServiceKey] = new CachedBotState(cachedState.State);
                return;
            }
        }

        /// <summary>
        /// Reset the state object to it's default form
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ClearStateAsync(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cachedState = context.Services.Get<CachedBotState>(this._contextServiceKey);
            if (cachedState != null)
            {
                context.Services[this._contextServiceKey] = new CachedBotState();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// IPropertyContainer.GetAsync() method gives IPropertyAccessor ability to get from it's container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="turnContext"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Task<T> GetPropertyAsync<T>(ITurnContext turnContext, string propertyName)
        {
            var cachedState = turnContext.Services.Get<CachedBotState>(this._contextServiceKey);
            if (cachedState.State.TryGetValue(propertyName, out object result))
            {
                return Task.FromResult<T>((T)result);
            }

            return Task.FromResult(default(T));
        }

        /// <summary>
        /// IPropertyContainer.DeleteAsync() method gives IPropertyAccessor ability to delete from it's container
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Task DeletePropertyAsync(ITurnContext turnContext, string propertyName)
        {
            var cachedState = turnContext.Services.Get<CachedBotState>(this._contextServiceKey);
            cachedState.State.Remove(propertyName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// IPropertyContainer.SetAsync() method gives IPropertyAccessor ability to set the value in it's container
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task SetPropertyAsync(ITurnContext turnContext, string propertyName, object value)
        {
            var cachedState = turnContext.Services.Get<CachedBotState>(this._contextServiceKey);
            cachedState.State[propertyName] = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Internal cached bot state
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

    }
}
