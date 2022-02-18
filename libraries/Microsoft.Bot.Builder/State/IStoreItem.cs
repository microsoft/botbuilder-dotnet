// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Exposes an ETag for concurrency control.
    /// </summary>
    public interface IStoreItem
    {
        /// <summary>
        /// Gets or sets the ETag for concurrency control.
        /// </summary>
        /// <value>The concurrency control ETag.</value>
        string ETag { get; set; }
    }

    /// <summary>
    /// Contains methods for retrieving or setting etag on <see cref="IStoreItem"/> or <see cref="JObject"/> objects.
    /// </summary>
    public static class ETagHelper
    {
        /// <summary>
        /// If the <paramref name="value"/> is an <see cref="IStoreItem"/> or <see cref="JObject"/>
        /// then the value of the "ETag" property is returned.  Otherwise, null.
        /// </summary>
        /// <param name="value">The object to check for an ETag.</param>
        /// <returns>The ETag property of the object passed in, if present.</returns>
        public static string GetETagOrNull(object value)
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

        /// <summary>
        /// Update the item's etag if it is an <see cref="IStoreItem"/> or <see cref="JObject"/>.
        /// </summary>
        /// <param name="item">The item to update the etag.</param>
        /// <param name="etag">The etag to use.</param>
        public static void UpdateETag(object item, string etag)
        {
            if (item is IStoreItem storeItem)
            {
                storeItem.ETag = etag;
            }
            else if (item is JObject asJobject)
            {
                asJobject["ETag"] = etag;
            }
        }
    }
}
