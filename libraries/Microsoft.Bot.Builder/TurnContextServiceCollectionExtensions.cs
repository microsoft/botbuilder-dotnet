// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Provides a set of convienience methods that extend the behavior of any <see cref="ITurnContextServiceCollection"/>. 
    /// </summary>
    public static class TurnContextServiceCollectionExtensions
    {
        
        /// <summary>
        /// Add a service using its full type name as the key.
        /// </summary>
        /// <typeparam name="TService">The type of service to be added.</typeparam>
        /// <param name="service">The service to add.</param>
        public static void Add<TService>(this ITurnContextServiceCollection serviceCollection, TService service) where TService : class =>
            serviceCollection.Add(typeof(TService).FullName, service);

        /// <summary>
        /// Get a service by type using its full type name as the key.
        /// </summary>
        /// <typeparam name="TService">The type of service to be retrieved.</typeparam>
        /// <returns>The service stored under the specified key.</returns>
        public static TService Get<TService>(this ITurnContextServiceCollection serviceCollection) where TService : class =>
            serviceCollection.Get<TService>(typeof(TService).FullName);

        /// <summary>
        /// Returns all entries in the collection of a specified type.
        /// </summary>
        /// <typeparam name="TService">The type of service to be found.</typeparam>
        /// <param name="serviceCollection">An <see cref="ITurnContextServiceCollection"/> to search for services in.</param>
        /// <returns>All instances of the requested service currently stored in the collection.</returns>
        public static IEnumerable<KeyValuePair<string, TService>> GetServices<TService>(this ITurnContextServiceCollection serviceCollection) where TService : class
        {
            foreach (var entry in serviceCollection)
            {
                if (entry.Value is TService service)
                {
                    yield return new KeyValuePair<string, TService>(entry.Key, service);
                }
            }
        }
    }
}
