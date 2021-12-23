// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [CollectionDefinition("CosmosDb")]
    public class CosmosDbCollectionDefinition : ICollectionFixture<CosmosDbFixture>
    {
    }
}
