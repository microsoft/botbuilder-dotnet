// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Technically should be buried under Integration folder, but options are independent
/// of any specific integration (at the moment).
/// </summary>
namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("BotFrameworkOptions")]
    public class BotFrameworkOptionsTests
    {
        [TestMethod]
        public void EnsureProperties()
        {
            BotFrameworkOptions options = new BotFrameworkOptions();
            Assert.IsNotNull(options.CredentialProvider);
            Assert.IsNull(options.HttpClient);
            Assert.IsNotNull(options.Middleware);
            Assert.IsNull(options.OnTurnError);
            Assert.IsNotNull(options.Paths);
#pragma warning disable 0618 // Disable the warning, as this test needs to be here.
            Assert.IsNotNull(options.State);
#pragma warning restore 0618
            Assert.IsNull(options.ConnectorClientRetryPolicy);
        }

        [TestMethod]
        public void EnsureDefaultPathsCorrect()
        {
            BotFrameworkOptions options = new BotFrameworkOptions();
            Assert.AreSame("/api", options.Paths.BasePath);
            Assert.AreSame("/messages", options.Paths.MessagesPath);
        }
    }
}
