// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Cosmos;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Cosmos DB Partitioned Storage Options.
    /// </summary>
    public class CosmosDbPartitionedStorageOptions
    {
        /// <summary>
        /// Gets or sets the CosmosDB endpoint.
        /// </summary>
        /// <value>
        /// The CosmosDB endpoint.
        /// </value>
        public string CosmosDbEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the authentication key for Cosmos DB.
        /// </summary>
        /// <value>
        /// The authentication key for Cosmos DB.
        /// </value>
        public string AuthKey { get; set; }

        /// <summary>
        /// Gets or sets the database identifier for Cosmos DB instance.
        /// </summary>
        /// <value>
        /// The database identifier for Cosmos DB instance.
        /// </value>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the container identifier.
        /// </summary>
        /// <value>
        /// The container identifier.
        /// </value>
        public string ContainerId { get; set; }

        /// <summary>
        /// Gets or sets the options for the CosmosClient.
        /// </summary>
        /// <value>
        /// The options for use with the CosmosClient.
        /// </value>
        public CosmosClientOptions CosmosClientOptions { get; set; }

        /// <summary>
        /// Gets or sets the throughput set when creating the Container. Defaults to 400.
        /// </summary>
        /// <value>
        /// Container throughput. Defaults to 400.
        /// </value>
        public int ContainerThroughput { get; set; } = 400;

        /// <summary>
        /// Gets or sets the suffix to be added to every key. <see cref="CosmosDbKeyEscape.EscapeKey(string)"/>.
        /// 
        /// Note: <see cref="CompatibilityMode"/> must be set to 'false' to use a KeySuffix.
        /// When KeySuffix is used, keys will NOT be truncated but an exception will be thrown if
        /// the key length is longer than allowed by CosmosDb.
        /// </summary>
        /// <value>
        /// String containing only valid CosmosDb key characters. (e.g. not: '\\', '?', '/', '#', '*').
        /// </value>
        public string KeySuffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to run in Compatibility Mode.
        /// Early versions of CosmosDb had a key length limit of 255.  Keys longer than this were
        /// truncated in <see cref="CosmosDbKeyEscape"/>.  This remains the default behavior, but
        /// can be overridden by setting CompatibilityMode to false.  This setting will also allow
        /// for using older collections where no PartitionKey was specified.
        /// 
        /// Note: CompatibilityMode cannot be 'true' if KeySuffix is used.
        /// </summary>
        /// <value>
        /// Currently, max key length for cosmosdb is 1023:
        /// https://docs.microsoft.com/en-us/azure/cosmos-db/concepts-limits#per-item-limits
        /// The default for backwards compatibility is 255 <see cref="CosmosDbKeyEscape.MaxKeyLength"/>.
        /// </value>
        public bool CompatibilityMode { get; set; } = true;
    }
}
