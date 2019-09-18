// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Slack.Tests
{
    public class SlackAdapterTests
    {
        [Fact]
        public void Constructor_Should_Fail_With_Null_Options()
        {
            Assert.Throws<ArgumentNullException>(() => new SlackAdapter(null));
        }
    }
}
