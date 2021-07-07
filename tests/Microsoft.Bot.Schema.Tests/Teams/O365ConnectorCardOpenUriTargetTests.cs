// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardOpenUriTargetTests
    {
        [Fact]
        public void O365ConnectorCardOpenUriTargetInits()
        {
            var os = "default";
            var uri = "www.bing.com";

            var openUriTarget = new O365ConnectorCardOpenUriTarget(os, uri);

            Assert.NotNull(openUriTarget);
            Assert.IsType<O365ConnectorCardOpenUriTarget>(openUriTarget);
            Assert.Equal(os, openUriTarget.Os);
            Assert.Equal(uri, openUriTarget.Uri);
        }
        
        [Fact]
        public void O365ConnectorCardOpenUriTargetInitsWithNoArgs()
        {
            var openUriTarget = new O365ConnectorCardOpenUriTarget();

            Assert.NotNull(openUriTarget);
            Assert.IsType<O365ConnectorCardOpenUriTarget>(openUriTarget);
        }
    }
}
