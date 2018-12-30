using Microsoft.Azure.Documents.Client;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// CosmosDB options for custom client.
    /// </summary>
    public class CosmosDbCustomClientOptions
    {
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
        /// Gets or sets the CosmosDB <see cref="Microsoft.Azure.Documents.Client.RequestOptions"/> that
        /// are passed when the document collection is created. Null is the default.
        /// </summary>
        /// <value>
        /// The set of options passed into
        /// <see cref="Microsoft.Azure.Documents.Client.DocumentClient.CreateDocumentCollectionIfNotExistsAsync(string, Microsoft.Azure.Documents.DocumentCollection, RequestOptions)"/>.
        /// </value>
        public RequestOptions DocumentCollectionRequestOptions { get; set; } = null;

        /// <summary>
        /// Gets or sets the CosmosDB <see cref="Microsoft.Azure.Documents.Client.RequestOptions"/>
        /// that are passed when the database is created. Null is the default.
        /// </summary>
        /// <value>
        /// The set of options passed into
        /// <see cref="Microsoft.Azure.Documents.Client.DocumentClient.CreateDatabaseIfNotExistsAsync(Microsoft.Azure.Documents.Database, RequestOptions)"/>.
        /// </value>
        public RequestOptions DatabaseCreationRequestOptions { get; set; } = null;
    }
}
