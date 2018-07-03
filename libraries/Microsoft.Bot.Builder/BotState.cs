// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Base class which manages details of automatic loading and saving of bot state.
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
        /// Initializes a new instance of the <see cref="BotState{TState}"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="propertyName">The name to use to load or save the state object.</param>
        /// <param name="keyDelegate">A function that can provide the key.</param>
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This middleware loads the state object on the leading edge of the middleware pipeline
        /// and persists the state object on the trailing edge.
        /// </remarks>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            await ReadToContextServiceAsync(context, cancellationToken).ConfigureAwait(false);
            await next(cancellationToken).ConfigureAwait(false);
            await WriteFromContextServiceAsync(context, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads state from storage.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If successful, the task result contains the state object, read from storage.</remarks>
        public virtual async Task<TState> ReadAsync(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = _keyDelegate(context);
            var items = await _storage.ReadAsync(new[] { key }, cancellationToken).ConfigureAwait(false);
            var state = items.Where(entry => entry.Key == key).Select(entry => entry.Value).OfType<TState>().FirstOrDefault();

            if (state == null)
            {
                state = new TState();
            }

            return state;
        }

        /// <summary>
        /// Writes state to storage.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="state">The state object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task WriteAsync(ITurnContext context, TState state, CancellationToken cancellationToken = default(CancellationToken))
        {
            var changes = new Dictionary<string, object>();

            if (state == null)
            {
                state = new TState();
            }

            var key = _keyDelegate(context);

            changes.Add(key, state);

            if (_settings.LastWriterWins)
            {
                foreach (var item in changes)
                {
                    if (item.Value is IStoreItem valueStoreItem)
                    {
                        valueStoreItem.ETag = "*";
                    }
                }
            }

            await _storage.WriteAsync(changes, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task ReadToContextServiceAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            var key = _keyDelegate(context);
            var items = await _storage.ReadAsync(new[] { key }, cancellationToken).ConfigureAwait(false);
            var state = items.Where(entry => entry.Key == key).Select(entry => entry.Value).OfType<TState>().FirstOrDefault();
            if (state == null)
            {
                state = new TState();
            }

            context.Services.Add(_propertyName, state);
        }

        protected virtual async Task WriteFromContextServiceAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            var state = context.Services.Get<TState>(_propertyName);
            await WriteAsync(context, state, cancellationToken).ConfigureAwait(false);
        }
    }
}
