using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    public class BotStateExtended : BotState
    {
        private readonly string _contextServiceKey;
        private readonly ITurnContextAwareStorage _turnContextAwareStorage;

        public BotStateExtended(ITurnContextAwareStorage turnContextAwareStorage, string contextServiceKey)
        : base(turnContextAwareStorage, contextServiceKey)
        {
            _turnContextAwareStorage = turnContextAwareStorage ?? throw new ArgumentNullException(nameof(turnContextAwareStorage));
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
            if (force || cachedState == null || cachedState.State == null)
            {
                object val;
                val = await _turnContextAwareStorage.ReadAsync(turnContext, cancellationToken).ConfigureAwait(false);

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
        /// Writes the state cache for this <see cref="BotState"/> to the storage layer.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="force">Optional, <c>true</c> to save the state cache to storage;
        /// or <c>false</c> to save state to storage only if a property in the cache has changed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public override async Task SaveChangesAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var cachedState = turnContext.TurnState.Get<CachedBotState>(_contextServiceKey);
            if (cachedState != null && (force || cachedState.IsChanged()))
            {
                await _turnContextAwareStorage.WriteAsync(turnContext, cachedState.State, cancellationToken).ConfigureAwait(false);
                cachedState.Hash = cachedState.ComputeHash(cachedState.State);
                return;
            }
        }

        public async Task<T> GetPropertyValueByNameAsync<T>(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.GetPropertyValueAsync<T>(turnContext, propertyName, cancellationToken).ConfigureAwait(false);
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
        public async Task SetPropertyValueByNameAsync(ITurnContext turnContext, string propertyName, object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.SetPropertyValueAsync(turnContext, propertyName, value, cancellationToken).ConfigureAwait(false);
        }

        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new ArgumentNullException("invalid activity-missing channelId");
            var userId = turnContext.Activity.From?.Id ?? throw new ArgumentNullException("invalid activity-missing From.Id");
            return $"{channelId}/users/{userId}";
        }
    }
}
