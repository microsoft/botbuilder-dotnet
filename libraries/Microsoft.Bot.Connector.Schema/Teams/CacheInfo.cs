// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// A cache info object which notifies Teams how long an object should be cached for.
    /// </summary>
    public class CacheInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheInfo"/> class.
        /// </summary>
        public CacheInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheInfo"/> class.
        /// </summary>
        /// <param name="cacheType">Type of Cache Info.</param>
        /// <param name="cacheDuration">Duration of the Cached Info.</param>
        public CacheInfo(string cacheType = default, int cacheDuration = default)
        {
            CacheType = cacheType;
            CacheDuration = cacheDuration;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets cache type.
        /// </summary>
        /// <value>The type of cache for this object.</value>
        [JsonPropertyName("cacheType")]
        public string CacheType { get; set; }

        /// <summary>
        /// Gets or sets cache duration.
        /// </summary>
        /// <value>The time in seconds for which the cached object should remain in the cache.</value>
        [JsonPropertyName("cacheDuration")]
        public int CacheDuration { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
