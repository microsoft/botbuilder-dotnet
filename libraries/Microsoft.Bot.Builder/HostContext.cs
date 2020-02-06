using System;
using System.Collections.Generic;
using System.Text;

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
        /// <remarks>This class is always a singleton for the process.</remarks>
        /// <value>
        /// The HostContext for the current process.
        /// </value>
        public static HostContext Current { get; private set; } = new HostContext();

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
        /// Set a value to the turn's context.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="key">The name of the service.</param>
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
        /// Set a value to the turn's context.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/>is null.</exception>
        public void Set<T>(T value)
            where T : class
        {
            Set(typeof(T).FullName, value);
        }
    }
}
