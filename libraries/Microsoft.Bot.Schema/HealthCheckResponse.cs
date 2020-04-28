// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets or sets the JSON for the Adaptive card to appear in the task
        /// module.
        /// </summary>
        /// <value>The health check results.</value>
        [JsonProperty(PropertyName = "healthResults")]
        public HealthResults HealthResults { get; set; }
    }
}
