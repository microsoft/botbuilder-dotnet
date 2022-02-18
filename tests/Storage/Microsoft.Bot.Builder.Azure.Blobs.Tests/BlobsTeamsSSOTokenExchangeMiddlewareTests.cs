// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Storage.Blobs;
using Microsoft.Bot.Builder.Tests.Common.Storage;

namespace Microsoft.Bot.Builder.Azure.Blobs.Tests
{
    public class BlobsTeamsSSOTokenExchangeMiddlewareTests : TeamsSSOTokenExchangeMiddlewareTestsBase
    {
        private const string ConnectionString = @"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        private const string ContainerName = "testteamssso";

        public override IStorage GetStorage()
        {
            var client = new BlobContainerClient(ConnectionString, ContainerName);
            client.DeleteIfExistsAsync().ConfigureAwait(false);
            client.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            return new BlobsStorage(new BlobsStorageOptions(ConnectionString, ContainerName));
        }
    }
}
