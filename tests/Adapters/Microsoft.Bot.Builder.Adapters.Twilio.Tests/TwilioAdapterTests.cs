// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
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
            var options = new Mock<ITwilioAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AuthToken = "Test";
            options.Object.AccountSid = "Test";

            Assert.Throws<Exception>(() => { new TwilioAdapter(options.Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AccountSid()
        {
            var options = new Mock<ITwilioAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AuthToken = "Test";
            options.Object.TwilioNumber = "Test";

            Assert.Throws<Exception>(() => { new TwilioAdapter(options.Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AuthToken()
        {
            var options = new Mock<ITwilioAdapterOptions>();
            options.SetupAllProperties();
            options.Object.TwilioNumber = "Test";
            options.Object.AccountSid = "Test";

            Assert.Throws<Exception>(() => { new TwilioAdapter(options.Object); });
        }

        [Fact]
        public void Constructor_WithArguments_Succeeds()
        {
            var options = new Mock<ITwilioAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AuthToken = "Test";
            options.Object.TwilioNumber = "Test";
            options.Object.AccountSid = "Test";

            Assert.NotNull(new TwilioAdapter(options.Object));
        }

        [Fact]
        public async void SendActivitiesAsync_Should_Fail_With_ActivityType_Not_Message()
        {
            var options = new Mock<ITwilioAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AuthToken = "Test";
            options.Object.TwilioNumber = "Test";
            options.Object.AccountSid = "Test";

            var twilioAdapter = new TwilioAdapter(options.Object);

            var activity = new Activity()
            {
                Type = ActivityTypes.Event,
            };

            Activity[] activities = { activity };

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await twilioAdapter.SendActivitiesAsync(new TurnContext(twilioAdapter, activity), activities, default);
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpRequest()
        {
            var options = new Mock<ITwilioAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AuthToken = "Test";
            options.Object.TwilioNumber = "Test";
            options.Object.AccountSid = "Test";

            var twilioAdapter = new TwilioAdapter(options.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(null, default(HttpResponse), default(IBot), default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpResponse()
        {
            var options = new Mock<ITwilioAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AuthToken = "Test";
            options.Object.TwilioNumber = "Test";
            options.Object.AccountSid = "Test";

            var twilioAdapter = new TwilioAdapter(options.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(default(HttpRequest), null, default(IBot), default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_Bot()
        {
            var options = new Mock<ITwilioAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AuthToken = "Test";
            options.Object.TwilioNumber = "Test";
            options.Object.AccountSid = "Test";

            var twilioAdapter = new TwilioAdapter(options.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(default(HttpRequest), default(HttpResponse), null, default(CancellationToken));
            });
        }
    }
}
