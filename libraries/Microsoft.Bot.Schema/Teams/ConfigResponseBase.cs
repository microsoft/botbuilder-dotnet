// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies Invoke response base including response type.
    /// </summary>
    public partial class ConfigResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigResponseBase"/> class.
        /// </summary>
        protected ConfigResponseBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigResponseBase"/> class.
        /// </summary>
        /// <param name="responseType"> response type for invoke.</param>
        protected ConfigResponseBase(string responseType)
        {
            ResponseType = responseType;
        }

        /// <summary>
        /// Gets or sets response type invoke request.
        /// </summary>
        /// <value> Invoke request response type.</value>
        [JsonProperty("responseType")]
        public string ResponseType { get; set; }
    }
}
