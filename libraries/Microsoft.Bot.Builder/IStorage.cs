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
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// DeleteAsync StoreItems from storage.
        /// </summary>
        /// <param name="keys">keys of the storeItems to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Exposes an ETag for concurrency control.
    /// </summary>
    public interface IStoreItem
    {
        /// <summary>
        /// Gets or sets the ETag for concurrency control.
        /// </summary>
        /// <value>The concurrency control ETag.</value>
        string ETag { get; set; }
    }

    /// <summary>
    /// Contains extension methods for <see cref="IStorage"/> objects.
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        /// Gets and strongly types a collection of <see cref="IStoreItem"/> objects from state storage.
        /// </summary>
        /// <typeparam name="TStoreItem">The type of item to get from storage.</typeparam>
        /// <param name="storage">The state storage.</param>
        /// <param name="keys">The collection of keys for the objects to get from storage.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains a dictionary of the
        /// strongly typed objects, indexed by the <paramref name="keys"/>.</remarks>
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
