// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Twilio.Tests
{
    public class TwilioAdapterTests
    {
        [Fact]
        public void Constructor_Should_Fail_With_Null_Options()
        {
            Assert.Throws<ArgumentNullException>(() => { new TwilioAdapter(null); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_TwilioNumber()
        {
            ITwilioAdapterOptions options = new MockTwilioOptions
            {
                TwilioNumber = null,
                AccountSid = "Test",
                AuthToken = "Test",
            };

            Assert.Throws<Exception>(() => { new TwilioAdapter(options); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AccountSid()
        {
            ITwilioAdapterOptions options = new MockTwilioOptions
            {
                TwilioNumber = "Test",
                AccountSid = null,
                AuthToken = "Test",
            };

            Assert.Throws<Exception>(() => { new TwilioAdapter(options); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AuthToken()
        {
            ITwilioAdapterOptions options = new MockTwilioOptions
            {
                TwilioNumber = "Test",
                AccountSid = "Test",
                AuthToken = null,
            };

            Assert.Throws<Exception>(() => { new TwilioAdapter(options); });
        }

        [Fact]
        public void Constructor_WithArguments_Succeeds()
        {
            ITwilioAdapterOptions options = new MockTwilioOptions
            {
                TwilioNumber = "Test",
                AccountSid = "Test",
                AuthToken = "Test",
            };

            Assert.NotNull(new TwilioAdapter(options));
        }

        private class MockTwilioOptions : ITwilioAdapterOptions
        {
            public string TwilioNumber { get; set; }

            public string AccountSid { get; set; }

            public string AuthToken { get; set; }

            public string ValidationUrl { get; set; }
        }
    }
}
