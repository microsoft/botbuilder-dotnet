// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Retrieve internal payload as a strongly typed value <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert the <paramref name="entity"/> into.</typeparam>
        /// <param name="entity">An entity to be converted into an instance of <typeparamref name="T"/>.</param>
        /// <returns>An instance of <typeparamref name="T"/> created from the specified <paramref name="entity"/>.</returns>
        public static T GetAs<T>(this Entity entity) =>
            // Clone the entity and convert to the specified type
            JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(entity));

        /// <summary>
        /// Set internal payload from a strongly typed value <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert into the <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The <see cref="Entity"/> to copy the values of the instance of <typeparamref name="T"/> into.</param>
        /// <param name="obj">An instance of <typeparamref name="T"/> that should have its values copied into the <paramref name="entity"/>.</param>
        public static void SetAs<T>(this Entity entity, T obj)
        {
            // Clone the value and store into this generic Entity type
            var clonedObj = JsonConvert.DeserializeObject<Entity>(JsonConvert.SerializeObject(obj));
            entity.Type = clonedObj.Type;
            entity.Properties = clonedObj.Properties;
        }
    }
}
