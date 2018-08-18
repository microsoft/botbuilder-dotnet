// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;

    public class AppInsightsService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsService"/> class.
        /// </summary>
        public AppInsightsService()
            : base(ServiceTypes.AppInsights)
        {
        }

        /// <summary>
        /// Gets or sets tenantId for the service (contoso.onmicrosoft.com).
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets subscriptionId for the appInsights service
        /// </summary>
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets resource group the appInsights service
        /// </summary>
        [JsonProperty("resourceGroup")]
        public string ResourceGroup { get; set; }

        /// <summary>
        /// Gets or sets instrumentation Key.
        /// </summary>
        [JsonProperty("instrumentationKey")]
        public string InstrumentationKey { get; set; }

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);
            this.InstrumentationKey = this.InstrumentationKey.Encrypt(secret);
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);
            this.InstrumentationKey = this.InstrumentationKey.Decrypt(secret);
        }
    }
}
