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
        /// Read StoreItems from storage.
        /// </summary>
        /// <param name="keys">keys of the storeItems to read.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Dictionary of Key/Value pairs.</returns>
        Task<IDictionary<string, object>> Read(string[] keys, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Dictionary of Key/Value pairs to write.
        /// </summary>
        /// <param name="changes">The changes to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task Write(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Delete StoreItems from storage.
        /// </summary>
        /// <param name="keys">keys of the storeItems to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task Delete(string[] keys, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IStoreItem
    {
        /// <summary>
        /// eTag for concurrency.
        /// </summary>
        string eTag { get; set; }
    }

    public static class StorageExtensions
    {
        /// <summary>
        /// Storage extension to Read as strong typed StoreItem objects.
        /// </summary>
        /// <typeparam name="StoreItemT"></typeparam>
        /// <param name="storage"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static async Task<IDictionary<string, StoreItemT>> Read<StoreItemT>(this IStorage storage, string[] keys, CancellationToken cancellationToken = default(CancellationToken))
            where StoreItemT : class
        {
            var storeItems = await storage.Read(keys, cancellationToken).ConfigureAwait(false);
            var values = new Dictionary<string, StoreItemT>(keys.Length);
            foreach (var entry in storeItems)
            {
                if (entry.Value is StoreItemT valueAsType)
                {
                    values.Add(entry.Key, valueAsType);
                }
            }

            return values;
        }
    }
}
