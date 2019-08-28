// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Values persisted for the lifetime of the turn as part of the <see cref="ITurnContext"/>.
    /// </summary>
    /// <remarks>
    /// TODO: add more details on what kind of values can/should be stored here, by whom and what the lifetime semantics are, etc.
    /// </remarks>
    public class TurnContextStateCollection : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TurnContextStateCollection"/> class.
        /// </summary>
        public TurnContextStateCollection()
        {
        }

        /// <summary>
        /// Gets a cached value by name from the turn's context.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="key">The name of the service.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <returns>The service object; or null if no service is registered by the key, or
        /// the retrieved object does not match the service type.</returns>
        public T Get<T>(string key)
            where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (TryGetValue(key, out var service))
            {
                if (service is T result)
                {
                    return result;
                }
            }

            // return null if either the key or type don't match
            return null;
        }

        /// <summary>
        /// Gets the default value by type from the turn's context.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <returns>The service object; or null if no default service of the type is registered.</returns>
        /// <remarks>The default service key is the <see cref="Type.FullName"/> of the service type.</remarks>
        public T Get<T>()
            where T : class
        {
            return Get<T>(typeof(T).FullName);
        }

        /// <summary>
        /// Adds a value to the turn's context.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="key">The name of the service.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/>
        /// is null.</exception>
        public void Add<T>(string key, T value)
            where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // note this can throw if teh key is already present
            base.Add(key, value);
        }

        /// <summary>
        /// Adds a value to the turn's context.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="value">The service object to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <remarks>The default service key is the <see cref="Type.FullName"/> of the service type.</remarks>
        public void Add<T>(T value)
            where T : class
        {
            Add(typeof(T).FullName, value);
        }

        public void Set<T>(T value)
            where T : class
        {
            this[typeof(T).FullName] = value;
        }
    }
}
