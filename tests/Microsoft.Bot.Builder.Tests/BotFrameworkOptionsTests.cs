// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration;
using Xunit;

/// <summary>
/// Technically should be buried under Integration folder, but options are independent
/// of any specific integration (at the moment).
/// </summary>
namespace Microsoft.Bot.Builder.Tests
{
    public class BotFrameworkOptionsTests
    {
        [Fact]
        public void EnsureProperties()
        {
            BotFrameworkOptions options = new BotFrameworkOptions();
            Assert.NotNull(options.CredentialProvider);
            Assert.Null(options.HttpClient);
            Assert.NotNull(options.Middleware);
            Assert.Null(options.OnTurnError);
            Assert.NotNull(options.Paths);
#pragma warning disable 0618 // Disable the warning, as this test needs to be here.
            Assert.NotNull(options.State);
#pragma warning restore 0618
            Assert.Null(options.ConnectorClientRetryPolicy);
        }

        [Fact]
        public void EnsureDefaultPathsCorrect()
        {
            BotFrameworkOptions options = new BotFrameworkOptions();
            Assert.Same("/api", options.Paths.BasePath);
            Assert.Same("/messages", options.Paths.MessagesPath);
        }
    }
}
