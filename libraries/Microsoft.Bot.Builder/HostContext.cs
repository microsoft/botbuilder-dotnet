// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents global objects for the runtime host.
    /// </summary>
    public class HostContext : Dictionary<string, object>
    {
        private HostContext()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Gets the HostContext for the current process.
        /// </summary>
        /// <remarks>This class is always a singleton for the process =.</remarks>
        /// <value>
        /// The HostContext for the current process.
        /// </value>
        public static HostContext Current { get; private set; } = new HostContext();

        /// <summary>
        /// Gets a cached value by name from the HostContext.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="key">The name of the object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <returns>The object; or null if no object is registered by the key, or
        /// the retrieved object does not match the type.</returns>
        public T Get<T>(string key)
            where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (TryGetValue(key, out var obj))
            {
                if (obj is T result)
                {
                    return result;
                }
            }

            // return null if either the key or type don't match
            return null;
        }

        /// <summary>
        /// Gets the default value by type from the HostContext.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object; or null if no default object of the type is registered.</returns>
        /// <remarks>The default object key is the <see cref="Type.FullName"/> of the object type.</remarks>
        public T Get<T>()
            where T : class
        {
            return Get<T>(typeof(T).FullName);
        }

        /// <summary>
        /// Set a value to the HostContext.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="key">The name of the object.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/>is null.</exception>
        public void Set<T>(string key, T value)
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

            this[key] = value;
        }

        /// <summary>
        /// Set a value to the HostContext.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/>is null.</exception>
        public void Set<T>(T value)
            where T : class
        {
            Set(typeof(T).FullName, value);
        }
    }
}
