// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Bot.Connector.Schema.Teams;
using Xunit;
using static Microsoft.Bot.Connector.Schema.Tests.Teams.TabsTestData;

namespace Microsoft.Bot.Connector.Schema.Tests.Teams
{
    public class TabSubmitDataTests
    {
        [Theory]
        [ClassData(typeof(TabSubmitDataTestData))]
        public void TabSubmitDataInits(string tabType, Dictionary<string, JsonElement> properties)
        {
            var submitData = new TabSubmitData()
            {
                Type = tabType,
                Properties = properties
            };

            Assert.NotNull(submitData);
            Assert.IsType<TabSubmitData>(submitData);
            Assert.Equal(tabType, submitData.Type);

            var dataProps = submitData.Properties;
            Assert.Equal(properties, dataProps);
            if (dataProps != null)
            {
                Assert.Equal(properties.Count, submitData.Properties.Count);
            }
        }
    }
}
