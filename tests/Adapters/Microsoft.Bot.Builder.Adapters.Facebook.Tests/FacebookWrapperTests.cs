// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
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
            const string requestHash = "SHA1=70C0E1B415F16D986EB839144FC85A941A5899C7";
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
        public async void SendMessageAsyncShouldReturnAnEmptyStringWithWrongPath()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[5];
            var facebookWrapper = new FacebookClientWrapper(_testOptions);
            var response = await facebookWrapper.SendMessageAsync("wrongPath", facebookMessage, null, default(CancellationToken));

            Assert.Equal(string.Empty, response);
        }

        [Fact]
        public async void SendMessageAsyncShouldThrowAnExceptionWithNullPath()
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
        public async void SendMessageAsyncShouldThrowAnExceptionWithNullPayload()
        {
            var facebookWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await facebookWrapper.SendMessageAsync("wrongPath", null, null, default(CancellationToken));
            });
        }

        [Fact]
        public async void VerifyWebhookAsyncShouldSendOkWhenVerified()
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
        public async void VerifyWebhookAsyncShouldSendUnauthorizedWhenNotVerified()
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
        public async void VerifyWebhookAsyncShouldThrowExceptionWithNullRequest()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.VerifyWebhookAsync(null, httpResponse.Object, default); });
        }

        [Fact]
        public async void VerifyWebhookAsyncShouldThrowExceptionWithNullResponse()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);
            var httpRequest = new Mock<HttpRequest>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.VerifyWebhookAsync(httpRequest.Object, null, default); });
        }

        [Fact]
        public async void PassThreadControlAsyncShouldThrowExceptionWithNullTargetAppId()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.PassThreadControlAsync(null, "fakeUserId", "Test Pass Thread Control", default); });
        }

        [Fact]
        public async void PassThreadControlAsyncShouldThrowExceptionWithNullUserId()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.PassThreadControlAsync("fakeAppId", null, "Test Pass Thread Control", default); });
        }

        [Fact]
        public async void RequestThreadControlAsyncShouldThrowExceptionWithNullUserId()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.RequestThreadControlAsync(null, "Test Pass Thread Control", default); });
        }

        [Fact]
        public async void TakeThreadControlAsyncShouldThrowExceptionWithNullUserId()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.TakeThreadControlAsync(null, "Test Pass Thread Control", default); });
        }

        [Fact]
        public async void PostToFacebookApiAsyncShouldThrowExceptionWithNullPostType()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.PostToFacebookApiAsync(null, "fakeContent", default); });
        }

        [Fact]
        public async void PostToFacebookApiAsyncShouldThrowExceptionWithNullContent()
        {
            var facebookClientWrapper = new FacebookClientWrapper(_testOptions);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookClientWrapper.PostToFacebookApiAsync("fakePostType", null, default); });
        }
    }
}
