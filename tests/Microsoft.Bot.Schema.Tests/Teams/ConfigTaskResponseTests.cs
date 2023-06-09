// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class ConfigTaskResponseTests
    {
        [Fact]
        public void ConfigTaskResponseInitWithNoArgs()
        {
            var configTaskResponse = new ConfigTaskResponse();

            Assert.NotNull(configTaskResponse);
            Assert.IsType<ConfigTaskResponse>(configTaskResponse);
            Assert.Equal("config", configTaskResponse.ResponseType);
        }
    }
}
