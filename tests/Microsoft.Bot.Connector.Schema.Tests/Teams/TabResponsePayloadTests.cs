// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Schema.Teams;
using Xunit;
using static Microsoft.Bot.Connector.Schema.Tests.Teams.TabsTestData;

namespace Microsoft.Bot.Connector.Schema.Tests.Teams
{
    public class TabResponsePayloadTests
    {
        [Theory]
        [ClassData(typeof(TabResponsePayloadTestData))]
        public void TabResponsePayloadInits(string tabType, TabResponseCards value, TabSuggestedActions suggestedActions)
        {
            var resPayload = new TabResponsePayload()
            {
                Type = tabType,
                Value = value,
                SuggestedActions = suggestedActions
            };

            Assert.NotNull(resPayload);
            Assert.IsType<TabResponsePayload>(resPayload);
        }
    }
}
