// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
<<<<<<< HEAD
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets or sets the JSON for the Adaptive card to appear in the task
        /// module.
=======
    /// <summary>
    /// Defines the structure that is returned as the result of a health check on the bot.
    /// The health check is sent to the bot as an <see cref="Activity"/> of type "invoke" and this class along with <see cref="HealthResults"/> defines the structure of the body of the response.
    /// The name of the invoke Activity is "healthCheck".
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets or sets the HealthResults for this health check call.
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
        /// </summary>
        /// <value>The health check results.</value>
        [JsonProperty(PropertyName = "healthResults")]
        public HealthResults HealthResults { get; set; }
    }
}
