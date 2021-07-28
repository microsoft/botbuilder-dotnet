// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;
using static Microsoft.Bot.Schema.Tests.Teams.TabsTestData;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TabRequestTests
    {
        [Theory]
        [ClassData(typeof(TabRequestTestData))]
        public void TabRequestInits(TabEntityContext tabEntityContext, TabContext tabContext, string state)
        {
            var tabRequest = new TabRequest()
            {
                TabEntityContext = tabEntityContext,
                Context = tabContext,
                State = state,
            };

            Assert.NotNull(tabRequest);
            Assert.IsType<TabRequest>(tabRequest);
            Assert.Equal(tabEntityContext, tabRequest.TabEntityContext);
            Assert.Equal(tabContext, tabRequest.Context);
            Assert.Equal(state, tabRequest.State);
        }
    }
}
