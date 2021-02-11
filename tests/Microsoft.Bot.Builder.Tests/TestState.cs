// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Tests
{
    public class TestState : IStoreItem
    {
        public string ETag { get; set; }

        public string Value { get; set; }
    }
}
