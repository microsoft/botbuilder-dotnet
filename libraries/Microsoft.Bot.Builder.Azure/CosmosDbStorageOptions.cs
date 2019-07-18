// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Cosmos DB Storage Options.
    /// </summary>
    public class CosmosDbStorageOptions
    {
        /// <summary>
        /// Gets or sets the partitionKey value.
        /// </summary>
        /// <value>
        /// The Partition Key.
        /// </value>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the CosmosDB endpoint.
        /// </summary>
        /// <value>
        /// The CosmosDB endpoint.
        /// </value>
        public Uri CosmosDBEndpoint { get; set; }

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
        /// Gets or sets the collection identifier.
        /// </summary>
        /// <value>
        /// The collection identifier.
        /// </value>
        public string CollectionId { get; set; }

        /// <summary>
        /// Gets or sets the connection policy configurator. This action allows you to customise the connection parameters.
        /// </summary>
        /// <remarks>You can use this delegate to
        /// further customize the connection to CosmosDB,
        /// such as setting connection mode, retry options, timeouts, and so on.
        /// See <see cref="Microsoft.Azure.Documents.Client.ConnectionPolicy"/>
        /// for more information.</remarks>
        /// <value>
        /// The connection policy configurator.
        /// </value>
        public Action<ConnectionPolicy> ConnectionPolicyConfigurator { get; set; } = (options) => { };

        /// <summary>
        /// Gets or sets the CosmosDB <see cref="Microsoft.Azure.Documents.Client.RequestOptions"/> that
        /// are passed when the document collection is created. Null is the default.
        /// </summary>
        /// <value>
        /// The set of options passed into
        /// <see cref="Microsoft.Azure.Documents.Client.DocumentClient.CreateDocumentCollectionIfNotExistsAsync(string, DocumentCollection, RequestOptions)"/>.
        /// </value>
        public RequestOptions DocumentCollectionRequestOptions { get; set; } = null;

        /// <summary>
        /// Gets or sets the CosmosDB <see cref="Microsoft.Azure.Documents.Client.RequestOptions"/> that
        /// are passed when the database is created. Null is the default.
        /// </summary>
        /// <value>
        /// The set of options passed into
        /// <see cref="Microsoft.Azure.Documents.Client.DocumentClient.CreateDatabaseIfNotExistsAsync(Database, RequestOptions)"/>.
        /// </value>
        public RequestOptions DatabaseCreationRequestOptions { get; set; } = null;
    }
}
