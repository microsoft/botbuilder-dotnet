// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardMultichoiceInputChoiceTests
    {
        [Fact]
        public void O365ConnectorCardMultichoiceInputChoiceInits()
        {
            var display = "C# in Depth";
            var value = "csharpInDepth";

            var choice = new O365ConnectorCardMultichoiceInputChoice(display, value);

            Assert.NotNull(choice);
            Assert.IsType<O365ConnectorCardMultichoiceInputChoice>(choice);
            Assert.Equal(display, choice.Display);
            Assert.Equal(value, choice.Value);
        }
        
        [Fact]
        public void O365ConnectorCardMultichoiceInputChoiceInitsWithNoArgs()
        {
            var choice = new O365ConnectorCardMultichoiceInputChoice();

            Assert.NotNull(choice);
            Assert.IsType<O365ConnectorCardMultichoiceInputChoice>(choice);
        }
    }
}
