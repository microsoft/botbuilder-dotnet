// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardActionBaseTests
    {
        [Fact]
        public void O365ConnectorCardActionBaseInits()
        {
            var type = "HttpPOST";
            var name = "OK";
            var id = "confirmation";

            var o365ConnectorCardActionBase = new O365ConnectorCardActionBase(type, name, id);

            Assert.NotNull(o365ConnectorCardActionBase);
            Assert.IsType<O365ConnectorCardActionBase>(o365ConnectorCardActionBase);
            Assert.Equal(type, o365ConnectorCardActionBase.Type);
            Assert.Equal(name, o365ConnectorCardActionBase.Name);
            Assert.Equal(id, o365ConnectorCardActionBase.Id);
        }
        
        [Fact]
        public void O365ConnectorCardActionBaseInitsWithNoArgs()
        {
            var o365ConnectorCardActionBase = new O365ConnectorCardActionBase();

            Assert.NotNull(o365ConnectorCardActionBase);
            Assert.IsType<O365ConnectorCardActionBase>(o365ConnectorCardActionBase);
        }
    }
}
