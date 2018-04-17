// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Core.Extensions;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    public class TestState : IStoreItem
    {
        public bool InPrompt { get; set; } = false;
        public string eTag { get; set; }
    }
}
