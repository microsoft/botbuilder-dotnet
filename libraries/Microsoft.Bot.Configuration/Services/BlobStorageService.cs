// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;

    /// <summary>
    /// Configuration properties for a Azure Blob Storage service.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class BlobStorageService : AzureService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageService"/> class.
        /// </summary>
        public BlobStorageService()
            : base(ServiceTypes.BlobStorage)
        {
        }

        /// <summary>
        /// Gets or sets connection string.
        /// </summary>
        /// <value>The connection String.</value>
        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets container.
        /// </summary>
        /// <value>The container.</value>
        [JsonProperty("container")]
        public string Container { get; set; }

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
