// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public interface IBotContext
    {
        BotAdapter Adapter { get; }

        /// <summary>
        /// Incoming request
        /// </summary>
        Activity Request { get; }

        /// <summary>
        /// 
        /// </summary>
        bool Responded { get; set; }

        Task SendActivity(params Activity[] activities);
        Task UpdateActivity(Activity activity);
        Task DeleteActivity(string activityId);

        /// <summary>
        /// Set the value associated with a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value to set.</param>
        void Set(string key, object value);

        /// <summary>
        /// Get a value by a key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value.</returns>
        object Get(string key);
    }

    public static partial class BotContextExtension
    {
        /// <summary>
        /// Set a value of a specific type on a context object.
        /// </summary>
        /// <typeparam name="ObjectT">The value's type.</typeparam>
        /// <param name="context">The context object.</param>
        /// <param name="value">The value.</param>
        /// <remarks>Uses the value type's name as the key.</remarks>
        public static void Set<ObjectT>(this IBotContext context, ObjectT value)
        {
            var key = $"{typeof(ObjectT).Namespace}.{typeof(ObjectT).Name}";
            context.Set(key, value);
        }

        /// <summary>
        /// Get a value of a specific type from a context object.
        /// </summary>
        /// <typeparam name="ObjectT">The value's type.</typeparam>
        /// <param name="context">The context object.</param>
        /// <param name="key">An optional lookup key. The default is the value type's name.</param>
        /// <returns>The value.</returns>
        public static ObjectT Get<ObjectT>(this IBotContext context, string key = null)
        {
            if (key == null)
                key = $"{typeof(ObjectT).Namespace}.{typeof(ObjectT).Name}";
            return (ObjectT)context.Get(key);
        }

    }
}
