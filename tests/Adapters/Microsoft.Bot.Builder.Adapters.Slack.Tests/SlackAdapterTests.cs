// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using Moq;
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

        [Fact]
        public void Constructor_Should_Fail_With_Null_Security_Mechanisms()
        {
            var slackAdapterOptions = new SlackAdapterOptions()
            {
                VerificationToken = null,
                ClientSigningSecret = null,
            };

            Assert.Throws<Exception>(() => new SlackAdapter(slackAdapterOptions));
        }

        [Fact]
        public void Constructor_Succeeds()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            Assert.NotNull(new SlackAdapter(options.Object));
        }
    }
}
