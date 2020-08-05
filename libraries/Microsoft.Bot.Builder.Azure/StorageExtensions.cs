// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Azure
{
    internal static class StorageExtensions
    {
        /// <summary>
        /// If the <paramref name="value"/> is an <see cref="IStoreItem"/> or <see cref="JObject"/>
        /// then the value of the "ETag" property is returned.  Otherwise, null.
        /// </summary>
        /// <param name="value">The object to check for an ETag.</param>
        /// <returns>The ETag property of the object passed in, if present.</returns>
        internal static string GetETagOrNull(object value)
        {
            if (value is IStoreItem asIStoreItem)
            {
                return asIStoreItem.ETag;
            }
            else if (value is JObject asJobject && asJobject.ContainsKey("ETag"))
            {
                return asJobject.Value<string>("ETag");
            }

            return null;
        }
    }
}
