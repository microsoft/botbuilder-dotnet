// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the base configuration for an Azure service.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class AzureService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureService"/> class.
        /// </summary>
        /// <param name="type">Identifies the service type.</param>
        public AzureService(string type)
            : base(type)
        {
        }

        /// <summary>
        /// Gets or sets tenant ID for the service, for example "contoso.onmicrosoft.com".
        /// </summary>
        /// <value>The Tenant Id.</value>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets subscription ID.
        /// </summary>
        /// <value>The subscription Id.</value>
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the resource group.
        /// </summary>
        /// <value>The Resource Group.</value>
        [JsonProperty("resourceGroup")]
        public string ResourceGroup { get; set; }

        /// <summary>
        /// Gets or sets the service name.
        /// </summary>
        /// <value>The Name of the Service.</value>
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }
    }
}
