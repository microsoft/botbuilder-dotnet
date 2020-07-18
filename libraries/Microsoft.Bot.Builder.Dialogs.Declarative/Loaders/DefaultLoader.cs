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
        /// <inheritdoc/>
        public virtual object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            return obj.ToObject(type, serializer);
        }
    }
}
