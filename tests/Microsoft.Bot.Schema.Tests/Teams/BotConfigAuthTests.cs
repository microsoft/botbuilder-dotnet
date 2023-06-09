// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class BotConfigAuthTests
    {
        [Fact]
        public void BotConfigAuthInitsWithNoArgs()
        {
            var botConfigAuthResponse = new BotConfigAuth();

            Assert.NotNull(botConfigAuthResponse);
            Assert.IsType<BotConfigAuth>(botConfigAuthResponse);
            Assert.Equal("auth", botConfigAuthResponse.Type);
        }
    }
}
