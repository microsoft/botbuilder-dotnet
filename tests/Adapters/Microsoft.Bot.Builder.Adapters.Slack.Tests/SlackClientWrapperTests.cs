// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Slack.Tests
{
    public class SlackClientWrapperTests
    {
        private readonly SlackClientWrapperOptions _testOptions = new SlackClientWrapperOptions("VerificationToken", "ClientSigningSecret", "BotToken");

        [Fact]
        public void VerifySignatureShouldReturnFalseWithNullParameters()
        {
            var slackApi = new SlackClientWrapper(_testOptions);

            Assert.False(slackApi.VerifySignature(null, null));
        }

        [Fact]
        public void VerifySignatureShouldReturnTrue()
        {
            var slackApi = new SlackClientWrapper(_testOptions);

            var body = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/MessageBody.json");

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers["X-Slack-Request-Timestamp"]).Returns("0001-01-01T00:00:00+00:00");
            httpRequest.SetupGet(req => req.Headers["X-Slack-Signature"]).Returns("V0=D213A711894A04CF10B2DAB9C6904436DCF1A7469E21C843BB4242E1F8E62EB0");

            Assert.True(slackApi.VerifySignature(httpRequest.Object, body));
        }
    }
}
