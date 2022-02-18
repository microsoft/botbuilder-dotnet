// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Builder.Azure.Cosmos.Tests
{
    [CollectionDefinition("CosmosDb Storage Tests Collection")]
    public class CosmosDbPartitionedStorageCollection : ICollectionFixture<CosmosDbPartitionStorageFixture>
    {
    }
}
