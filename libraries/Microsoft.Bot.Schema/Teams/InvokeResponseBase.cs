// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies Invoke response base including response type.
    /// </summary>
    public partial class InvokeResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeResponseBase"/> class.
        /// </summary>
        protected InvokeResponseBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeResponseBase"/> class.
        /// </summary>
        /// <param name="responseType"> response type for invoke.</param>
        protected InvokeResponseBase(string responseType)
        {
            ResponseType = responseType;
        }

        /// <summary>
        /// Gets or sets response type invoke request.
        /// </summary>
        /// <value> Invoke request response type.</value>
        [JsonProperty("responseType")]
        public string ResponseType { get; set; }

        /// <summary>
        /// Gets or sets response cache Info.
        /// </summary>
        /// <value> Value of cache info. </value>
        [JsonProperty(PropertyName = "cacheInfo")]
        public CacheInfo CacheInfo { get; set; }
    }
}
