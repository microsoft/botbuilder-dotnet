// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;
using static Microsoft.Bot.Schema.Tests.Teams.TabsTestData;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TabResponseTests
    {
        [Theory]
        [ClassData(typeof(TabResponseTestData))]
        public void TabResponseInits(TabResponsePayload tab)
        {
            var tabResponse = new TabResponse()
            {
                Tab = tab
            };

            Assert.NotNull(tabResponse);
            Assert.IsType<TabResponse>(tabResponse);
            Assert.Equal(tab, tabResponse.Tab);
        }
    }
}
