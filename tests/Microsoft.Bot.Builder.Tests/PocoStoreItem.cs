// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Tests
{
    public class PocoStoreItem : IStoreItem
    {
        public string ETag { get; set; }

        public string Id { get; set; }

        public int Count { get; set; }
    }
}
