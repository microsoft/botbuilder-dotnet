using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Bot.Builder.Adapters.Webex;
using Microsoft.Bot.Schema;
using Moq;
using Thrzn41.WebexTeams;
using Thrzn41.WebexTeams.Version1;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Webex.Tests
{
    public class WebexHelperTests
    {
        [Fact]
        public void PayloadToActivity_Should_Return_Null_With_Null_Payload()
        {
            Assert.Null(WebexHelper.PayloadToActivity(null));
        }

        [Fact(Skip = "just nope")]
        public void PayloadToActivity_Should_Return_Activity()
        {
            var payload = new WebhookEventData(); // TODO: create payload

            Assert.NotNull(WebexHelper.PayloadToActivity(payload));
        }

        [Fact]
        public void ValidateSignature_Should_Fail_With_Missing_Signature()
        {
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(string.Empty);

            Assert.Throws<Exception>(() =>
            {
                WebexHelper.ValidateSignature("test_secret", httpRequest.Object, "{}");
            });
        }

        [Fact]
        public void ValidateSignature_Should_Return_False()
        {
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("wrong_signature");
            httpRequest.Object.Body = Stream.Null;

            Assert.False(WebexHelper.ValidateSignature("test_secret", httpRequest.Object, "{}"));
        }

        [Fact]
        public void DecryptedMessageToActivityAsync_Should_Return_Null_With_Null_Payload()
        {
            Func<string, CancellationToken?, Task<TeamsResult<Message>>> callback = default;
            Assert.Null(WebexHelper.DecryptedMessageToActivityAsync(null, callback).Result);
        }
    }
}
