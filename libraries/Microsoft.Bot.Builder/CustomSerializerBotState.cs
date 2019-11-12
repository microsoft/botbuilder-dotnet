// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Defines a state management object and automates the reading and writing of associated state
    /// properties to a storage layer.
    ///
    /// This BotState implementation is useful for when the IStorage JsonSerializer's
    /// TypeNameHandling = TypeNameHandling.None.
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
    public abstract class CustomSerializerBotState : BotState
    {
        private readonly string _contextServiceKey;
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSerializerBotState"/> class.
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
        public CustomSerializerBotState(IStorage storage, string contextServiceKey)
            : base(storage, contextServiceKey)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _contextServiceKey = contextServiceKey ?? throw new ArgumentNullException(nameof(contextServiceKey));
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
        public override async Task LoadAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
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

                if (val is IDictionary<string, object> asDictionary)
                {
                    turnContext.TurnState[_contextServiceKey] = new CachedBotState(asDictionary);
                }
                else if (val is JObject asJobject)
                {
                    // If types are not used by storage serialization, and Newtonsoft is the serializer
                    // the item found will be a JObject.
                    turnContext.TurnState[_contextServiceKey] = new CachedBotState(asJobject.ToObject<IDictionary<string, object>>());
                }
                else if (val == null)
                {
                    // This is the case where the dictionary did not exist in the store.
                    turnContext.TurnState[_contextServiceKey] = new CachedBotState();
                }
                else
                {
                    // This should never happen
                    throw new InvalidOperationException("Data is not in the correct format for BotState.");
                }
            }
        }

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
        protected override Task<T> GetPropertyValueAsync<T>(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
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

            // If types are not used by storage serialization, and Newtonsoft is the serializer,
            // use Newtonsoft to convert the object to the type expected.
            if (cachedState.State[propertyName] is JObject obj)
            {
                return Task.FromResult(obj.ToObject<T>());
            }
            else if (cachedState.State[propertyName] is JArray jarray)
            {
                return Task.FromResult(jarray.ToObject<T>());
            }

            // if there is no value, this will throw, to signal to IPropertyAccesor that a default value should be computed
            // This allows this to work with value types
            return Task.FromResult((T)cachedState.State[propertyName]);
        }
    }
}
