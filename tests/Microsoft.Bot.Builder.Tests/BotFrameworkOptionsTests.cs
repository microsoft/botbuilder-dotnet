
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
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
    class BotFrameworkOptionsTests
    {
        [TestMethod]
        public void EnsureProperties()
        {
            BotFrameworkOptions options = new BotFrameworkOptions();
            Assert.IsNotNull(options.CredentialProvider);
            Assert.IsNotNull(options.HttpClient);
            Assert.IsNotNull(options.Middleware);
            Assert.IsNull(options.OnTurnError);
            Assert.IsNotNull(options.Paths);
            Assert.IsNotNull(options.State);
            Assert.IsNotNull(options.ConnectorClientRetryPolicy);
            Assert.IsNotNull(options.HttpClient);
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
