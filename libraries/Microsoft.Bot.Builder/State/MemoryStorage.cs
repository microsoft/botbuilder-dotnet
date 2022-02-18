// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A storage layer that uses an in-memory dictionary.
    /// </summary>
    public class MemoryStorage : IStorage
    {
        private static readonly JsonSerializer StateJsonSerializer = new JsonSerializer()
        {
            TypeNameHandling = TypeNameHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
        };

        // If a JsonSerializer is not provided during construction, this will be the default static JsonSerializer.
        private readonly JsonSerializer _stateJsonSerializer;
        private readonly Dictionary<string, JObject> _memory;
        private readonly object _syncroot = new object();
        private int _eTag = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorage"/> class.
        /// </summary>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.All.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// </param>
        /// <param name="dictionary">A pre-existing dictionary to use; or null to use a new one.</param>
        public MemoryStorage(JsonSerializer jsonSerializer, Dictionary<string, JObject> dictionary = null)
        {
            _stateJsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _memory = dictionary ?? new Dictionary<string, JObject>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorage"/> class.
        /// </summary>
        /// <param name="dictionary">A pre-existing dictionary to use; or null to use a new one.</param>
        public MemoryStorage(Dictionary<string, JObject> dictionary = null)
            : this(StateJsonSerializer, dictionary)
        {
        }

        /// <summary>
        /// Deletes storage items from storage.
        /// </summary>
        /// <param name="keys">keys of the <see cref="IStoreItem"/> objects to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ReadAsync(string[], CancellationToken)"/>
        /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    _memory.Remove(key);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads storage items from storage.
        /// </summary>
        /// <param name="keys">keys of the <see cref="IStoreItem"/> objects to read.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// the items read, indexed by key.</remarks>
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
        public Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            var storeItems = new Dictionary<string, object>(keys.Length);
            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    if (_memory.TryGetValue(key, out var state))
                    {
                        if (state != null)
                        {
                            storeItems.Add(key, state.ToObject<object>(_stateJsonSerializer));
                        }
                    }
                }
            }

            return Task.FromResult<IDictionary<string, object>>(storeItems);
        }

        /// <summary>
        /// Writes storage items to storage.
        /// </summary>
        /// <param name="changes">The items to write, indexed by key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="ReadAsync(string[], CancellationToken)"/>
        public Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            lock (_syncroot)
            {
                foreach (var change in changes)
                {
                    var newValue = change.Value;

                    var oldStateETag = default(string);

                    if (_memory.TryGetValue(change.Key, out var oldState))
                    {
                        if (oldState != null && oldState.TryGetValue("ETag", StringComparison.OrdinalIgnoreCase, out var etag))
                        {
                            oldStateETag = etag.Value<string>();
                        }
                    }

                    var newState = newValue != null ? JObject.FromObject(newValue, _stateJsonSerializer) : null;

                    // Set ETag if applicable
                    SetETagFromNewValue(newValue, newState, oldStateETag);

                    _memory[change.Key] = newState;
                }
            }

            return Task.CompletedTask;
        }

        private void SetETagFromNewValue(object newValue, JObject newState, string oldStateETag)
        {
            if (newValue is IStoreItem newStoreItem)
            {
                newStoreItem.ETag = SetETag(newState, oldStateETag, newStoreItem.ETag);
            }
            else if (newValue is JObject asJobject && asJobject.ContainsKey("ETag"))
            {
                asJobject["ETag"] = SetETag(newState, oldStateETag, asJobject.Value<string>("ETag"));
            }
        }

        private string SetETag(JObject state, string oldStateETag, string newEtag)
        {
            if (oldStateETag != null
                    &&
               newEtag != "*"
                    &&
               newEtag != oldStateETag)
            {
                throw new State.StoreItemETagException($"Etag conflict. \r\n\r\nOriginal: {newEtag}\r\nCurrent: {oldStateETag}");
            }

            if (newEtag?.Length == 0)
            {
                throw new State.StoreItemETagException("etag empty");
            }

            var etag = (_eTag++).ToString(CultureInfo.InvariantCulture);
            state["ETag"] = etag;
            return etag;
        }
    }
}
