// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;

    public class AzureService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureService"/> class.
        /// </summary>
        public AzureService(string type)
            : base(type)
        {
        }

        /// <summary>
        /// Gets or sets tenantId for the service (contoso.onmicrosoft.com).
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets subscriptionId.
        /// </summary>
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets resource group.
        /// </summary>
        [JsonProperty("resourceGroup")]
        public string ResourceGroup { get; set; }

        /// <summary>
        /// Gets or sets ServiceName.
        /// </summary>
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }
    }
}
