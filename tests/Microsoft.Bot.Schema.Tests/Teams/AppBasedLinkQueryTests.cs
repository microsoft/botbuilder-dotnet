// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class AppBasedLinkQueryTests
    {
        [Fact]
        public void AppBasedLinkQueryInits()
        {
            var url = "http://example.com";
            var state = "magicCode";

            var appBasedLinkQuery = new AppBasedLinkQuery(url)
            {
                State = state
            };

            Assert.NotNull(appBasedLinkQuery);
            Assert.IsType<AppBasedLinkQuery>(appBasedLinkQuery);
            Assert.Equal(url, appBasedLinkQuery.Url);
            Assert.Equal(state, appBasedLinkQuery.State);
        }

        [Fact]
        public void AppBasedLinkQueryInitsWithNoArgs()
        {
            var appBasedLinkQuery = new AppBasedLinkQuery();

            Assert.NotNull(appBasedLinkQuery);
            Assert.IsType<AppBasedLinkQuery>(appBasedLinkQuery);
        }
    }
}
