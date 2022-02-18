// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder.Tests.Common.Storage;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Cosmos.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - CosmosDB TeamsSSOTokenExchangeMiddleware")]
    [Collection("CosmosDb Storage Tests Collection")]
    public class CosmosDbTeamsSSOTokenExchangeMiddlewareTests : TeamsSSOTokenExchangeMiddlewareTestsBase, IAsyncLifetime
    {
        private const string CosmosCollectionName = "testteamssso";

        public CosmosDbTeamsSSOTokenExchangeMiddlewareTests(CosmosDbPartitionStorageFixture fixture)
        {
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [IgnoreOnNoEmulatorFact]
        public override Task TokenExchanged_OnTurnFires()
        {
            return base.TokenExchanged_OnTurnFires();
        }

        [IgnoreOnNoEmulatorFact]
        public override Task TokenExchanged_SecondSendsInvokeResponse()
        {
            return base.TokenExchanged_SecondSendsInvokeResponse();
        }

        [IgnoreOnNoEmulatorFact]
        public override Task TokenNotExchanged_DirectLineChannel()
        {
            return base.TokenNotExchanged_DirectLineChannel();
        }

        [IgnoreOnNoEmulatorFact]
        public override Task TokenNotExchanged_PreconditionFailed()
        {
            return base.TokenNotExchanged_PreconditionFailed();
        }

        public override IStorage GetStorage()
        {
            using (var client = new CosmosClient(
                    CosmosDbTestConstants.CosmosServiceEndpoint,
                    CosmosDbTestConstants.CosmosAuthKey,
                    new CosmosClientOptions()))
            {
                client.GetContainer(CosmosDbTestConstants.CosmosDatabaseName, CosmosCollectionName).DeleteContainerAsync();
                client.CreateDatabaseIfNotExistsAsync(CosmosDbTestConstants.CosmosDatabaseName).GetAwaiter().GetResult();
            }

            return new CosmosDbPartitionedStorage(
                new CosmosDbPartitionedStorageOptions
                {
                    AuthKey = CosmosDbTestConstants.CosmosAuthKey,
                    ContainerId = CosmosCollectionName,
                    CosmosDbEndpoint = CosmosDbTestConstants.CosmosServiceEndpoint,
                    DatabaseId = CosmosDbTestConstants.CosmosDatabaseName,
                });
        }
    }
}
