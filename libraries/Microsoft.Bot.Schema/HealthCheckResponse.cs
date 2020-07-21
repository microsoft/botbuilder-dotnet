// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines the structure that is returned as the result of a health check on the bot.
    /// The health check is sent to the bot as an <see cref="Activity"/> of type "invoke" and this class along with <see cref="HealthResults"/> defines the structure of the body of the response.
    /// The name of the invoke Activity is "healthCheck".
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets or sets the HealthResults for this health check call.
        /// </summary>
        /// <value>The health check results.</value>
        [JsonProperty(PropertyName = "healthResults")]
        public HealthResults HealthResults { get; set; }
    }
}
