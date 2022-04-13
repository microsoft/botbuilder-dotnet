// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - CosmosDB")]
    public class CosmosDbStorageTests
    {
        private readonly Uri _cosmosDBEndpoint = new Uri("https://localhost:8081");
        private readonly string _authKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private CosmosDbStorage _storage;
        private readonly Mock<IDocumentClient> _documentClient = new Mock<IDocumentClient>();

        public interface IDocumentQueryMock<T> : IDocumentQuery<T>, IOrderedQueryable<T>
        {
        }

        [Fact]
        public void ConstructorValidation()
        {
            var documentClient = Mock.Of<IDocumentClient>();

            // Should work.
            _ = new CosmosDbStorage(
                cosmosDbStorageOptions: new CosmosDbStorageOptions
                {
                    CosmosDBEndpoint = _cosmosDBEndpoint,
                    AuthKey = _authKey,
                    DatabaseId = "DatabaseId",
                    CollectionId = "CollectionId",
                },
                jsonSerializer: JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));

            // No Options. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage(null));
            Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage(documentClient, null));

            // No Endpoint. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions()
            {
                CosmosDBEndpoint = null,
            }));

            // No Auth Key. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions()
            {
                CosmosDBEndpoint = _cosmosDBEndpoint,
                AuthKey = null,
            }));

            // No Database Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions()
            {
                CosmosDBEndpoint = _cosmosDBEndpoint,
                AuthKey = _authKey,
                DatabaseId = null,
            }));
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(documentClient, new CosmosDbCustomClientOptions()
            {
                DatabaseId = null,
            }));

            // No Collection Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions()
            {
                CosmosDBEndpoint = _cosmosDBEndpoint,
                AuthKey = _authKey,
                DatabaseId = "DatabaseId",
                CollectionId = null,
            }));
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(documentClient, new CosmosDbCustomClientOptions()
            {
                DatabaseId = "DatabaseId",
                CollectionId = null,
            }));

            // No JsonSerializer. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage(
                new CosmosDbStorageOptions()
                {
                    CosmosDBEndpoint = _cosmosDBEndpoint,
                    AuthKey = _authKey,
                    DatabaseId = "DatabaseId",
                    CollectionId = "CollectionId",
                }, null));

            // No DocumentClient. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage(null, new CosmosDbCustomClientOptions()
            {
                DatabaseId = "DatabaseId",
                CollectionId = "CollectionId",
            }));
        }

        [Fact]
        public void SanitizeKey()
        {
            const string validKey = "Abc12345";
            var sanitizedKey = CosmosDbStorage.SanitizeKey(validKey);

            Assert.Equal(validKey, sanitizedKey);
        }

        [Fact]
        public async void ReadAsyncValidation()
        {
            InitStorage();

            // No keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.ReadAsync(null, CancellationToken.None));

            // Empty keys. Should return empty.
            var empty = await _storage.ReadAsync(new string[] { }, CancellationToken.None);
            Assert.Empty(empty);
        }

        [Fact]
        public async void ReadAsync()
        {
            InitStorage();

            var documentQuery = new Mock<IDocumentQueryMock<CosmosDbStorage.DocumentStoreItem>>();
            var provider = new Mock<IQueryProvider>();

            var resources = new List<CosmosDbStorage.DocumentStoreItem>
            {
                new CosmosDbStorage.DocumentStoreItem
                {
                    ReadlId = "ReadlId",
                    ETag = "ETag1",
                    Document = JObject.Parse("{ \"ETag\":\"ETag2\" }")
                },
            };

            var response = new FeedResponse<CosmosDbStorage.DocumentStoreItem>(resources);

            provider.Setup(e => e.CreateQuery<CosmosDbStorage.DocumentStoreItem>(It.IsAny<Expression>()))
                .Returns(documentQuery.Object);
            documentQuery.SetupSequence(_ => _.HasMoreResults)
                .Returns(true)
                .Returns(false);
            documentQuery.Setup(e => e.ExecuteNextAsync<CosmosDbStorage.DocumentStoreItem>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            documentQuery.Setup(e => e.Provider)
                .Returns(provider.Object);
            _documentClient.Setup(e => e.CreateDocumentQuery<CosmosDbStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<SqlQuerySpec>(), It.IsAny<FeedOptions>()))
                .Returns(documentQuery.Object);
            
            var items = await _storage.ReadAsync(new string[] { "key" }, CancellationToken.None);

            Assert.Single(items);
            _documentClient.Verify(e => e.CreateDocumentQuery<CosmosDbStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<SqlQuerySpec>(), It.IsAny<FeedOptions>()), Times.Once);
            documentQuery.Verify(e => e.ExecuteNextAsync<CosmosDbStorage.DocumentStoreItem>(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void WriteAsyncValidation()
        {
            InitStorage();

            // No changes. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.WriteAsync(null, CancellationToken.None));

            // Empty changes. Should return.
            await _storage.WriteAsync(new Dictionary<string, object>(), CancellationToken.None);
        }

        [Fact]
        public async void WriteAsync()
        {
            InitStorage();

            _documentClient.Setup(e => e.UpsertDocumentAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<RequestOptions>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
            _documentClient.Setup(e => e.ReplaceDocumentAsync(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()));

            var changes = new Dictionary<string, object>
            {
                { "key1", new CosmosDbStorage.DocumentStoreItem() },
                { "key2", new CosmosDbStorage.DocumentStoreItem { ETag = "*" } },
                { "key3", new CosmosDbStorage.DocumentStoreItem { ETag = "ETag" } },
            };

            await _storage.WriteAsync(changes, CancellationToken.None);

            _documentClient.Verify(e => e.UpsertDocumentAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<RequestOptions>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _documentClient.Verify(e => e.ReplaceDocumentAsync(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void WriteAsyncEmptyTagFailure()
        {
            InitStorage();

            var changes = new Dictionary<string, object>
            {
                { "key", new CosmosDbStorage.DocumentStoreItem { ETag = string.Empty } },
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _storage.WriteAsync(changes, CancellationToken.None));
        }

        [Fact]
        public async void DeleteAsyncValidation()
        {
            InitStorage();

            // No keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteAsync(null, CancellationToken.None));

            // Empty keys. Should return.
            await _storage.DeleteAsync(new string[] { }, CancellationToken.None);
        }

        [Fact]
        public async void DeleteAsync()
        {
            InitStorage();

            _documentClient.Setup(e => e.DeleteDocumentAsync(It.IsAny<Uri>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()));

            await _storage.DeleteAsync(new string[] { "key" }, CancellationToken.None);

            _documentClient.Verify(e => e.DeleteDocumentAsync(It.IsAny<Uri>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private void InitStorage(CosmosDbCustomClientOptions storageOptions = default)
        {
            var connectionPolicy = new ConnectionPolicy();
            var documentCollection = new DocumentCollection();
            var resourceResponse = new ResourceResponse<DocumentCollection>(documentCollection);

            _documentClient.SetupGet(e => e.ConnectionPolicy).Returns(connectionPolicy);
            _documentClient.Setup(e => e.CreateDatabaseIfNotExistsAsync(It.IsAny<Database>(), It.IsAny<RequestOptions>()));
            _documentClient.Setup(e => e.CreateDocumentCollectionIfNotExistsAsync(It.IsAny<Uri>(), It.IsAny<DocumentCollection>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(resourceResponse);

            var options = storageOptions ?? new CosmosDbCustomClientOptions
            {
                DatabaseId = "DatabaseId",
                CollectionId = "CollectionId",
            };
            _storage = new CosmosDbStorage(_documentClient.Object, options);
        }
    }
}
