// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardActionCardTests
    {
        [Fact]
        public void O365ConnectorCardActionCardInits()
        {
            var name = "Set due date";
            var id = "dueDate";
            var inputs = new List<O365ConnectorCardInputBase>() { new O365ConnectorCardInputBase("dateInput") };
            var actions = new List<O365ConnectorCardActionBase>() { new O365ConnectorCardActionBase("ActionCard") };

            var o365ConnectorCardActionCard = new O365ConnectorCardActionCard("ActionCard", name, id, inputs, actions);

            Assert.NotNull(o365ConnectorCardActionCard);
            Assert.IsType<O365ConnectorCardActionCard>(o365ConnectorCardActionCard);
            Assert.Equal(name, o365ConnectorCardActionCard.Name);
            Assert.Equal(id, o365ConnectorCardActionCard.Id);
            Assert.Equal(inputs, o365ConnectorCardActionCard.Inputs);
            Assert.Equal(actions, o365ConnectorCardActionCard.Actions);
        }
        
        [Fact]
        public void O365ConnectorCardActionCardInitsWithNoArgs()
        {
            var o365ConnectorCardActionCard = new O365ConnectorCardActionCard();

            Assert.NotNull(o365ConnectorCardActionCard);
            Assert.IsType<O365ConnectorCardActionCard>(o365ConnectorCardActionCard);
        }
    }
}
