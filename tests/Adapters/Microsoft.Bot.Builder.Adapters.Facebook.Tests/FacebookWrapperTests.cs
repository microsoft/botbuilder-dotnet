// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Facebook.Tests
{
    public class FacebookWrapperTests
    {
        private readonly FacebookClientWrapperOptions _testOptions = new FacebookClientWrapperOptions("TestVerifyToken", "TestAppSecret", "TestAccessToken");

        [Fact]
        public void GetAppSecretProofShouldAlwaysReturnAStringWith64Characters()
        {
            const int secretProofLength = 64;
            var facebookWrapper = new FacebookClientWrapper(_testOptions);
            var secretProof = facebookWrapper.GetAppSecretProof();

            Assert.NotNull(secretProof);
            Assert.Equal(secretProofLength, secretProof.Length);
        }

        [Fact]
        public void VerifySignatureShouldThrowErrorWithNullRequest()
        {
            var facebookWrapper = new FacebookClientWrapper(_testOptions);

            Assert.Throws<ArgumentNullException>(() => { facebookWrapper.VerifySignature(null, string.Empty); });
        }

        [Fact]
        public void VerifySignatureShouldReturnTrueWithValidRequestHash()
        {
            const string requestHash = "13870D954C7CB3A6725C7C8DC58260E6EEE77D538DAFEA1A3703DCC2AE21E97F";
            var facebookWrapper = new FacebookClientWrapper(_testOptions);
            var request = new Mock<HttpRequest>();
            var stringifyBody = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/RequestResponse.json");

            request.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(requestHash);

            Assert.True(facebookWrapper.VerifySignature(request.Object, stringifyBody));
        }

        [Fact]
        public void VerifySignatureShouldReturnFalseWithInvalidRequestHash()
        {
            const string requestHash = "FakeHash";
            var facebookWrapper = new FacebookClientWrapper(_testOptions);
            var request = new Mock<HttpRequest>();

            request.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(requestHash);

            Assert.False(facebookWrapper.VerifySignature(request.Object, string.Empty));
        }

        [Fact]
        public async Task SendMessageAsyncShouldThrowAnExceptionWithWrongPath()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[5];
            var facebookWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<HttpRequestException>(async () => 
            {
                await facebookWrapper.SendMessageAsync("wrongPath", facebookMessage, null, default(CancellationToken));
            });
        }

        [Fact]
        public async Task SendMessageAsyncShouldThrowAnExceptionWithNullPath()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[5];
            var facebookWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await facebookWrapper.SendMessageAsync(null, facebookMessage, null, default(CancellationToken));
            });
        }

        [Fact]
        public async Task SendMessageAsyncShouldThrowAnExceptionWithNullPayload()
        {
            var facebookWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await facebookWrapper.SendMessageAsync("wrongPath", null, null, default(CancellationToken));
            });
        }

        [Fact]
        public async Task VerifyWebhookAsyncShouldSendOkWhenVerified()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            httpRequest.SetupGet(req => req.Query[It.IsAny<string>()]).Returns("TestVerifyToken");
            httpResponse.SetupAllProperties();
            httpResponse.Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                {
                    if (length > 0)
                    {
                        var actual = Encoding.UTF8.GetString(data);
                    }
                });

            await facebookClientWrapper.VerifyWebhookAsync(httpRequest.Object, httpResponse.Object, default);

            Assert.True(httpResponse.Object.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task VerifyWebhookAsyncShouldSendUnauthorizedWhenNotVerified()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            httpRequest.SetupGet(req => req.Query[It.IsAny<string>()]).Returns("WrongVerifyToken");
            httpResponse.SetupAllProperties();
            httpResponse.Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                {
                    if (length > 0)
                    {
                        var actual = Encoding.UTF8.GetString(data);
                    }
                });

            await facebookClientWrapper.VerifyWebhookAsync(httpRequest.Object, httpResponse.Object, default);

            Assert.True(httpResponse.Object.StatusCode == (int)HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task VerifyWebhookAsyncShouldThrowExceptionWithNullRequest()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.VerifyWebhookAsync(null, httpResponse.Object, default); });
        }

        [Fact]
        public async Task VerifyWebhookAsyncShouldThrowExceptionWithNullResponse()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);
            var httpRequest = new Mock<HttpRequest>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.VerifyWebhookAsync(httpRequest.Object, null, default); });
        }

        [Fact]
        public async Task PassThreadControlAsyncShouldThrowExceptionWithNullTargetAppId()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.PassThreadControlAsync(null, "fakeUserId", "Test Pass Thread Control", default); });
        }

        [Fact]
        public async Task PassThreadControlAsyncShouldThrowExceptionWithNullUserId()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.PassThreadControlAsync("fakeAppId", null, "Test Pass Thread Control", default); });
        }

        [Fact]
        public async Task RequestThreadControlAsyncShouldThrowExceptionWithNullUserId()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.RequestThreadControlAsync(null, "Test Pass Thread Control", default); });
        }

        [Fact]
        public async Task TakeThreadControlAsyncShouldThrowExceptionWithNullUserId()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.TakeThreadControlAsync(null, "Test Pass Thread Control", default); });
        }

        [Fact]
        public async Task PostToFacebookApiAsyncShouldThrowExceptionWithNullPostType()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.PostToFacebookApiAsync(null, "fakeContent", default); });
        }

        [Fact]
        public async Task PostToFacebookApiAsyncShouldThrowExceptionWithNullContent()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.PostToFacebookApiAsync("fakePostType", null, default); });
        }
    }
}
