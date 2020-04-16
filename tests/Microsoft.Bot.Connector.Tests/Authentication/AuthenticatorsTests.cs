// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Moq;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class AuthenticatorsTests
    {
        [Fact]
        public async Task Authenticators_None_Good()
        {
            MockFail(out var mockOne, out var failOne);
            MockFail(out var mockTwo, out var failTwo);

            IAuthenticator subject = new Authenticators(mockOne.Object, mockTwo.Object);

            try
            {
                var actual = await subject.GetTokenAsync();
                Assert.True(false);
            }
            catch (Exception actual)
            {
                Assert.NotEqual(failOne, actual);
                Assert.Equal(failTwo, actual);
            }
        }

        [Fact]
        public async Task Authenticators_First_Good()
        {
            MockGood(out var mockOne, out var goodOne);
            MockNone(out var mockTwo);

            IAuthenticator subject = new Authenticators(mockOne.Object, mockTwo.Object);

            var actual = await subject.GetTokenAsync();

            Assert.Equal(goodOne, actual);
        }

        [Fact]
        public async Task Authenticators_Second_Good()
        {
            MockFail(out var mockOne, out var _);
            MockGood(out var mockTwo, out var goodTwo);

            IAuthenticator subject = new Authenticators(mockOne.Object, mockTwo.Object);

            var actual = await subject.GetTokenAsync();

            Assert.Equal(goodTwo, actual);
        }

        private static void MockNone(out Mock<IAuthenticator> mock)
        {
            mock = new Mock<IAuthenticator>(MockBehavior.Strict);
        }

        private static void MockGood(out Mock<IAuthenticator> mock, out AuthenticatorResult good)
        {
            mock = new Mock<IAuthenticator>(MockBehavior.Strict);
            good = new AuthenticatorResult();
            mock.Setup(a => a.GetTokenAsync(It.IsAny<bool>())).ReturnsAsync(good).Verifiable();
        }

        private static void MockFail(out Mock<IAuthenticator> mock, out Exception fail)
        {
            mock = new Mock<IAuthenticator>(MockBehavior.Strict);
            fail = new Exception();
            mock.Setup(a => a.GetTokenAsync(It.IsAny<bool>())).ThrowsAsync(fail).Verifiable();
        }
    }
}
