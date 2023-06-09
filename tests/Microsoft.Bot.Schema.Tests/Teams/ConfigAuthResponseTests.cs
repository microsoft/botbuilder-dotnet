// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class ConfigAuthResponseTests
    {
        [Fact]
        public void ConfigAuthResponseInitWithNoArgs()
        {
            var configAuthResponse = new ConfigAuthResponse();

            Assert.NotNull(configAuthResponse);
            Assert.IsType<ConfigAuthResponse>(configAuthResponse);
            Assert.Equal("config", configAuthResponse.ResponseType);
        }
    }
}
