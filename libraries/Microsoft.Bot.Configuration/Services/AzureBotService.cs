// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;

    public class AzureBotService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBotService"/> class.
        /// </summary>
        public AzureBotService()
            : base(ServiceTypes.AzureBot)
        {
        }

        /// <summary>
        /// Gets or sets tenantId for the service (contoso.onmicrosoft.com).
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets subscriptionId for the bot.
        /// </summary>
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets resource group the bot is in.
        /// </summary>
        [JsonProperty("resourceGroup")]
        public string ResourceGroup { get; set; }
    }
}
