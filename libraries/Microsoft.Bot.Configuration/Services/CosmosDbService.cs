// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;

    public class CosmosDbService : AzureService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbService "/> class.
        /// </summary>
        public CosmosDbService()
            : base(ServiceTypes.CosmosDB)
        {
        }

        /// <summary>
        /// Gets or sets connection string.
        /// </summary>
        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets database.
        /// </summary>
        [JsonProperty("database")]
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets collection.
        /// </summary>
        [JsonProperty("collection")]
        public string Collection { get; set; }

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);

            if (!string.IsNullOrEmpty(this.ConnectionString))
            {
                this.ConnectionString = this.ConnectionString.Encrypt(secret);
            }
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);

            if (!string.IsNullOrEmpty(this.ConnectionString))
            {
                this.ConnectionString = this.ConnectionString.Decrypt(secret);
            }
        }
    }
}
