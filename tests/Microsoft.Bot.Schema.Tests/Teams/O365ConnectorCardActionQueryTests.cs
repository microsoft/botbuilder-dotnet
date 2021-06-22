// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardActionQueryTests
    {
        [Fact]
        public void O365ConnectorCardActionQueryInits()
        {
            var body = "body";
            var actionId = "ActionCard";

            var actionQuery = new O365ConnectorCardActionQuery(body, actionId);

            Assert.NotNull(actionQuery);
            Assert.IsType<O365ConnectorCardActionQuery>(actionQuery);
            Assert.Equal(body, actionQuery.Body);
            Assert.Equal(actionId, actionQuery.ActionId);
        }
        
        [Fact]
        public void O365ConnectorCardActionQueryInitsWithNoArgs()
        {
            var actionQuery = new O365ConnectorCardActionQuery();

            Assert.NotNull(actionQuery);
            Assert.IsType<O365ConnectorCardActionQuery>(actionQuery);
        }
    }
}
