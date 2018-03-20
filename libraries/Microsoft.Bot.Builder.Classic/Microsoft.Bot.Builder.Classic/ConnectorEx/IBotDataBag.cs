// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Classic.Dialogs
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
