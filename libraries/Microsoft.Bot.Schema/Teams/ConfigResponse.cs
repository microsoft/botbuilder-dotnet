// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Envelope for Config Response Payload.
    /// </summary>
    /// <typeparam name="T">The first generic type parameter.</typeparam>.
    public partial class ConfigResponse<T> : InvokeResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigResponse{T}"/> class.
        /// </summary>
        public ConfigResponse()
            : base("config")
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the response to the config message.
        /// Possible values for the config type include: 'auth'or 'task'.
        /// </summary>
        /// <value>
        /// Response to a config request.
        /// </value>
        [JsonProperty(PropertyName = "config")]
        public T Config { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
