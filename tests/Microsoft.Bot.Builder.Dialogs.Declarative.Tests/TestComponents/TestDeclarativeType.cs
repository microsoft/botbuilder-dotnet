// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests.TestComponents
{
    internal class TestDeclarativeType
    {
        public const string Kind = "TestKind";

        public TestDeclarativeType(string data = null)
        {
            Data = data;
        }

        public string Data { get; set; }
    }
}
