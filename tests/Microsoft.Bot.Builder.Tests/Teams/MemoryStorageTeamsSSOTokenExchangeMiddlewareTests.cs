// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Tests.Common.Storage;

namespace Microsoft.Bot.Builder.Tests
{
    public class MemoryStorageTeamsSSOTokenExchangeMiddlewareTests : TeamsSSOTokenExchangeMiddlewareTestsBase
    {
        public override IStorage GetStorage()
        {
            return new MemoryStorage();
        }
    }
}
