// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardViewActionTests
    {
        [Fact]
        public void O365ConnectorCardViewActionInits()
        {
            var type = "ViewAction";
            var name = "Custom Action";
            var id = "customAction";
            var target = new List<string>() { "https://example.com" };

            var viewAction = new O365ConnectorCardViewAction(type, name, id, target);

            Assert.NotNull(viewAction);
            Assert.IsType<O365ConnectorCardViewAction>(viewAction);
            Assert.Equal(name, viewAction.Name);
            Assert.Equal(id, viewAction.Id);
            Assert.Equal(target, viewAction.Target);
            Assert.Equal(1, viewAction.Target.Count);
        }
        
        [Fact]
        public void O365ConnectorCardViewActionInitsWithNoArgs()
        {
            var viewAction = new O365ConnectorCardViewAction();

            Assert.NotNull(viewAction);
            Assert.IsType<O365ConnectorCardViewAction>(viewAction);
        }
    }
}
