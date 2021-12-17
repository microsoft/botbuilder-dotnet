// Copyright (c) Microsoft Corporation. All righ reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    public class IConfigurationBuilderExtensionsTests
    {
        [Fact]
        [Trait("TestCategory ", "AI")]
        [Trait("TestCategory ", "QnAMaker")]
        public void QnAMakerSettings_GetUserJsonSettingFiles()
        {
            var builder = new ConfigurationBuilder();
            var config = builder.Build();
            var botRoot = config.GetValue<string>("root") ?? ".";
            var region = config.GetValue<string>("region") ?? "westus";
            var environment = config.GetValue<string>("environment") ?? "development";

            builder.UseQnAMakerSettings(botRoot, region, environment);

            var source = builder.Sources[1] as Microsoft.Extensions.Configuration.Json.JsonConfigurationSource;
            Assert.Equal("qnamaker.settings.development.westus.json", source.Path);
        }
    }
}
