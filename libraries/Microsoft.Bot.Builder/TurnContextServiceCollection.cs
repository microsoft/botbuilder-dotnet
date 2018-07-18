// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents a set of collection of services associated with the <see cref="ITurnContext"/>.
    /// </summary>
    /// <remarks>
    /// TODO: add more details on what kind of services can/should be stored here, by whom and what the lifetime semantics are, etc.
    /// </remarks>
    public class TurnContextServiceCollection : Dictionary<string, object>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TurnContextServiceCollection"/> class.
        /// </summary>
        public TurnContextServiceCollection()
        {
        }

        /// <summary>
        /// Gets a service by name from the turn's context.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="key">The name of the service.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <returns>The service object; or null if no service is registered by the key, or
        /// the retrieved object does not match the service type.</returns>
        public TService Get<TService>(string key)
            where TService : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (TryGetValue(key, out var service))
            {
                if (service is TService result)
                {
                    return result;
                }
            }

            // return null if either the key or type don't match
            return null;
        }

        /// <summary>
        /// Gets the default service by type from the turn's context.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>The service object; or null if no default service of the type is registered.</returns>
        /// <remarks>The default service key is the <see cref="Type.FullName"/> of the service type.</remarks>
        public TService Get<TService>()
            where TService : class
        {
            return Get<TService>(typeof(TService).FullName);
        }

        /// <summary>
        /// Adds a service to the turn's context.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="key">The name of the service.</param>
        /// <param name="service">The service object to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="service"/>
        /// is null.</exception>
        public void Add<TService>(string key, TService service)
            where TService : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            // note this can throw if teh key is already present
            base.Add(key, service);
        }

        /// <summary>
        /// Adds a service to the turn's context.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="service">The service object to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
        /// <remarks>The default service key is the <see cref="Type.FullName"/> of the service type.</remarks>
        public void Add<TService>(TService service)
            where TService : class
        {
            Add(typeof(TService).FullName, service);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var entry in Values)
            {
                if (entry is IDisposable disposableService)
                {
                    disposableService.Dispose();
                }
            }
        }
    }
}
