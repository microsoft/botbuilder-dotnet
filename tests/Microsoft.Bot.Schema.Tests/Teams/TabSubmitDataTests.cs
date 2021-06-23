// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Xunit;
using static Microsoft.Bot.Schema.Tests.Teams.TabsTestData;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TabSubmitDataTests
    {
        [Theory]
        [ClassData(typeof(TabSubmitDataTestData))]
        public void TabSubmitDataInits(string tabType, JObject properties)
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
