// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Slack.Tests
{
    public class SlackClientWrapperTests
    {
        [Fact]
        public void VerifySignatureShouldReturnFalseWithNullParameters()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new SlackClientWrapper(options.Object);

            Assert.False(slackApi.VerifySignature(null, null));
        }

        [Fact]
        public void VerifySignatureShouldReturnTrue()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new SlackClientWrapper(options.Object);

            var body = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageBody.json");

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers["X-Slack-Request-Timestamp"]).Returns("0001-01-01T00:00:00+00:00");
            httpRequest.SetupGet(req => req.Headers["X-Slack-Signature"]).Returns("V0=B53049CFD9F1DD2818B6DD952C905A2D38055BCB55958DF560B8E5E5AE4D62E0");

            Assert.True(slackApi.VerifySignature(httpRequest.Object, body));
        }
    }
}
