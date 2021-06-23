// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;
using static Microsoft.Bot.Schema.Tests.Teams.TabsTestData;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TabSuggestedActionsTests
    {
        [Theory]
        [ClassData(typeof(TabSuggestedActionsTestData))]
        public void TabSuggestedActionsInits(IList<CardAction> actions)
        {
            var suggestedActions = new TabSuggestedActions()
            {
                Actions = actions
            };

            Assert.NotNull(suggestedActions);
            Assert.IsType<TabSuggestedActions>(suggestedActions);
            Assert.Equal(actions, suggestedActions.Actions);
        }
    }
}
