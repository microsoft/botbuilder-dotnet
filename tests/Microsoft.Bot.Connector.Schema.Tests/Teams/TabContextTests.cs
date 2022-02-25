// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Connector.Schema.Tests.Teams
{
    public class TabContextTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("hi")]
        public void TabContextInits(string theme)
        {
            var tabContext = new TabContext()
            {
                Theme = theme
            };

            Assert.NotNull(tabContext);
            Assert.IsType<TabContext>(tabContext);
            Assert.Equal(theme, tabContext.Theme);
        }
    }
}
