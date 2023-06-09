// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class ConfigResponseTests
    {
        [Fact]
        public void ConfigResponseInitsWithNoArgs()
        {
            var configResponse = new ConfigResponse<BotConfigAuth>();

            Assert.NotNull(configResponse);
            Assert.IsType<ConfigResponse<BotConfigAuth>>(configResponse);
        }
    }
}
