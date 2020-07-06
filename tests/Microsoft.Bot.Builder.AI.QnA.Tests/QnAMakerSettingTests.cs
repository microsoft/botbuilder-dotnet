// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    [TestClass]
    public class QnAMakerSettingTests
    {
        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public void QnAMakerSetting()
        {
            var builder = new ConfigurationBuilder();
            var config = builder.Build();
            var botRoot = config.GetValue<string>("root") ?? ".";
            var region = config.GetValue<string>("region") ?? "westus";
            var environment = config.GetValue<string>("environment") ?? "development";

            builder.UseQnAMakerSettings(botRoot, region, environment);

            var source = builder.Sources[1] as Microsoft.Extensions.Configuration.Json.JsonConfigurationSource;
            Assert.AreEqual(source.Path, "qnamaker.settings.development.westus.json");
        }
    }
}
