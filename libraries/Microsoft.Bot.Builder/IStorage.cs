// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IStorage
    {
        /// <summary>
        /// ReadAsync StoreItems from storage.
        /// </summary>
        /// <param name="keys">keys of the storeItems to read.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Dictionary of Key/Value pairs.</returns>
        Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Dictionary of Key/Value pairs to write.
        /// </summary>
        /// <param name="changes">The changes to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// DeleteAsync StoreItems from storage.
        /// </summary>
        /// <param name="keys">keys of the storeItems to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IStoreItem
    {
        /// <summary>
        /// Gets or sets the ETag for concurrency control.
        /// </summary>
        string ETag { get; set; }
    }

    public static class StorageExtensions
    {
        /// <summary>
        /// Storage extension to ReadAsync as strong typed StoreItem objects.
        /// </summary>
        /// <typeparam name="TStoreItem"></typeparam>
        /// <param name="storage"></param>
        /// <param name="keys"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<IDictionary<string, TStoreItem>> ReadAsync<TStoreItem>(this IStorage storage, string[] keys, CancellationToken cancellationToken = default(CancellationToken))
            where TStoreItem : class
        {
            var storeItems = await storage.ReadAsync(keys, cancellationToken).ConfigureAwait(false);
            var values = new Dictionary<string, TStoreItem>(keys.Length);
            foreach (var entry in storeItems)
            {
                if (entry.Value is TStoreItem valueAsType)
                {
                    values.Add(entry.Key, valueAsType);
                }
            }

            return values;
        }
    }
}
