// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardOpenUriTests
    {
        [Fact]
        public void O365ConnectorCardOpenUriInits()
        {
            var type = "OpenUri";
            var name = "Go to Bing";
            var id = "goToBing";
            var targets = new List<O365ConnectorCardOpenUriTarget>() { new O365ConnectorCardOpenUriTarget("default", "www.bing.com") };

            var openUri = new O365ConnectorCardOpenUri(type, name, id, targets);

            Assert.NotNull(openUri);
            Assert.IsType<O365ConnectorCardOpenUri>(openUri);
            Assert.Equal(name, openUri.Name);
            Assert.Equal(id, openUri.Id);
            Assert.Equal(targets, openUri.Targets);
            Assert.Equal(1, openUri.Targets.Count);
        }
        
        [Fact]
        public void O365ConnectorCardOpenUriInitsWithNoArgs()
        {
            var openUri = new O365ConnectorCardOpenUri();

            Assert.NotNull(openUri);
            Assert.IsType<O365ConnectorCardOpenUri>(openUri);
        }
    }
}
