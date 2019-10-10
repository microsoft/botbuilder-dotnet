// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Facebook.Tests
{
    public class FacebookWrapperTests
    {
        private readonly FacebookAdapterOptions _testOptions = new FacebookAdapterOptions("TestVerifyToken", "TestAppSecret", "TestAccessToken");

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
            const string requestHash = "SHA1=32ECC29689D860D68A713FF5BA8D7B787C5E8C80";
            var facebookWrapper = new FacebookClientWrapper(_testOptions);
            var request = new Mock<HttpRequest>();
            var stringifyBody = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\RequestResponse.json");

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
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[5];
            var facebookWrapper = new FacebookClientWrapper(_testOptions);
            var response = await facebookWrapper.SendMessageAsync("wrongPath", facebookMessage, null, default(CancellationToken));

            Assert.Equal(string.Empty, response);
        }

        [Fact]
        public async void SendMessageAsyncShouldThrowAnExceptionWithNullPath()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\FacebookMessages.json");
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
    }
}
