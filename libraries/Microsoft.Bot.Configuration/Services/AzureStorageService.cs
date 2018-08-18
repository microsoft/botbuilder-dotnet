// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;

    public class AzureStorageService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageService"/> class.
        /// </summary>
        public AzureStorageService()
            : base(ServiceTypes.AzureStorage)
        {
        }

        /// <summary>
        /// Gets or sets tenantId for the service (contoso.onmicrosoft.com).
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets subscriptionId for the service.
        /// </summary>
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets resource group for the service.
        /// </summary>
        [JsonProperty("resourceGroup")]
        public string ResourceGroup { get; set; }

        /// <summary>
        /// Gets or sets connection string.
        /// </summary>
        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);
            this.ConnectionString = this.ConnectionString.Encrypt(secret);
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);
            this.ConnectionString = this.ConnectionString.Decrypt(secret);
        }
    }
}
