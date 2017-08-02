using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A property bag of bot data.
    /// </summary>
    public interface IBotDataBag
    {
        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="IBotDataBag"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Checks if data bag contains a value with specified key
        /// </summary>
        /// <param name="key">The key.</param>
        bool ContainsKey(string key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if the <see cref="IBotDataBag"/> contains an element with the specified key; otherwise, false.</returns>
        bool TryGetValue<T>(string key, out T value);

        /// <summary>
        /// Adds the specified key and value to the bot data bag.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        void SetValue<T>(string key, T value);

        /// <summary>
        /// Removes the specified key from the bot data bag.
        /// </summary>
        /// <param name="key">They key of the element to remove</param>
        /// <returns>True if removal of the key is successful; otherwise, false</returns>
        bool RemoveValue(string key);

        /// <summary>
        /// Removes all of the values from data bag.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Helper methods.
    /// </summary>
    public static partial class Extensions
    {
        [System.Obsolete(@"Use GetValue<T> instead", false)]
        public static T Get<T>(this IBotDataBag bag, string key)
        {
            T value;
            if (!bag.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException(key);
            }

            return value;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="bag">The bot data bag.</param>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a KeyNotFoundException.</returns>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/></exception>
        public static T GetValue<T>(this IBotDataBag bag, string key)
        {
            T value;
            if (!bag.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException(key);
            }

            return value;
        }

        /// <summary>
        /// Gets the value associated with the specified key or a default value if not found.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="bag">The bot data bag.</param>
        /// <param name="key">The key of the value to get or set.</param>
        /// <param name="defaultValue">The value to return if the key is not present</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, <paramref name="defaultValue"/>
        /// is returned </returns>
        public static T GetValueOrDefault<T>(this IBotDataBag bag, string key, T defaultValue = default(T))
        {
            T value;
            if (!bag.TryGetValue(key, out value))
            {
                value = defaultValue;
            }

            return value;
        }
    }
}
