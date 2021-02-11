// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Loaders
{
    /// <summary>
    /// Defines the contract for a deserializer from a JToken object to another type.
    /// </summary>
    public interface ICustomDeserializer
    {
        /// <summary>
        /// The method that loads the JToken object to a requested type.
        /// </summary>
        /// <param name="obj">The JToken object to deserialize.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> that will be used when creating the object.</param>
        /// <param name="type">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the JToken value.</returns>
        object Load(JToken obj, JsonSerializer serializer, Type type);
    }
}
