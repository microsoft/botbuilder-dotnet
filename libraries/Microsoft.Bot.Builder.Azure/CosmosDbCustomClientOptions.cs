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
        /// Gets or sets the CosmosDB <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.client.requestoptions?view=azure-dotnet"/> that
        /// are passed when the document collection is created. Null is the default.
        /// </summary>
        /// <value>
        /// The set of options passed into
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.idocumentclient.createdocumentcollectionifnotexistsasync?view=azure-dotnet"/>.
        /// </value>
        public RequestOptions DocumentCollectionRequestOptions { get; set; } = null;

        /// <summary>
        /// Gets or sets the CosmosDB <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.client.requestoptions?view=azure-dotnet"/>
        /// that are passed when the database is created. Null is the default.
        /// </summary>
        /// <value>
        /// The set of options passed into
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.client.documentclient.createdatabaseifnotexistsasync?view=azure-dotnet"/>.
        /// </value>
        public RequestOptions DatabaseCreationRequestOptions { get; set; } = null;
    }
}
