// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class JwtTokenProviderFactoryTests
    {
        private const string TestAppId = "foo";

        [Fact]
        public void CanCreateAzureServiceTokenProvider()
        {
            var sut = new JwtTokenProviderFactory();
            var tokenProvider = sut.CreateAzureServiceTokenProvider(TestAppId);
            Assert.NotNull(tokenProvider);
        }

        [Fact]
        public void CanCreateAzureServiceTokenProviderWithCustomHttpClient()
        {
            using (var customHttpClient = new HttpClient())
            {
                var sut = new JwtTokenProviderFactory();
                var tokenProvider = sut.CreateAzureServiceTokenProvider(TestAppId, customHttpClient);
                Assert.NotNull(tokenProvider);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CannotCreateAzureServiceTokenProviderWithoutAppId(string appId)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var sut = new JwtTokenProviderFactory();
                _ = sut.CreateAzureServiceTokenProvider(appId);
            });
        }
    }
}
