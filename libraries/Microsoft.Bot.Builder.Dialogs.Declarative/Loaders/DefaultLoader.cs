// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Loaders
{
    /// <summary>
    /// A default loader for deserializing JToken objects.
    /// </summary>
    public class DefaultLoader : ICustomDeserializer
    {
        /// <summary>
        /// The method that loads the JToken object to a requested type.
        /// </summary>
        /// <param name="obj">The JToken object to deserialize.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> that will be used when creating the object.</param>
        /// <param name="type">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the JToken value.</returns>
        public virtual object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            return obj.ToObject(type, serializer);
        }
    }
}
