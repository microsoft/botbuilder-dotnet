// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared;
using Microsoft.Bot.Builder.TestBot.Shared.CognitiveModels;
using Microsoft.Bot.Builder.TestBot.Shared.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared.Services;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class MainDialogTests : BotTestBase
    {
        private readonly BookingDialog _mockBookingDialog;
        private readonly Mock<ILogger<MainDialog>> _mockLogger;

        public MainDialogTests(ITestOutputHelper output)
            : base(output)
        {
            _mockLogger = new Mock<ILogger<MainDialog>>();

            var mockFlightBookingService = new Mock<IFlightBookingService>();
            mockFlightBookingService
                .Setup(x => x.BookFlight(It.IsAny<BookingDetails>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _mockBookingDialog = SimpleMockFactory.CreateMockDialog<BookingDialog>(null, new Mock<GetBookingDetailsDialog>().Object, mockFlightBookingService.Object).Object;
        }

        [Fact]
        public void DialogConstructor()
        {
            var sut = new MainDialog(_mockLogger.Object, null, _mockBookingDialog);

            Assert.Equal("MainDialog", sut.Id);
            Assert.IsType<TextPrompt>(sut.FindDialog("TextPrompt"));
            Assert.NotNull(sut.FindDialog("BookingDialog"));
            Assert.IsType<WaterfallDialog>(sut.FindDialog("WaterfallDialog"));
        }

        [Fact]
        public async Task ShowsMessageIfLuisNotConfigured()
        {
            // Arrange
            var sut = new MainDialog(_mockLogger.Object, null, _mockBookingDialog);
            var testClient = new DialogTestClient(Channels.Test, sut, middlewares: new[] { new XUnitDialogTestLogger(Output) });

            // Act/Assert
            var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");
            Assert.Equal("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", reply.Text);

            reply = testClient.GetNextReply<IMessageActivity>();
            Assert.Equal("What can I help you with today?", reply.Text);
        }

        [Fact]
        public async Task ShowsPromptIfLuisIsConfigured()
        {
            // Arrange
            var sut = new MainDialog(_mockLogger.Object, SimpleMockFactory.CreateMockLuisRecognizer<IRecognizer>(null).Object, _mockBookingDialog);
            var testClient = new DialogTestClient(Channels.Test, sut, middlewares: new[] { new XUnitDialogTestLogger(Output) });

            // Act/Assert
            var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");
            Assert.Equal("What can I help you with today?", reply.Text);
        }

        [Theory]
        [InlineData("I want to book a flight", "BookFlight", "BookingDialog mock invoked")]
        [InlineData("What's the weather like?", "GetWeather", "TODO: get weather flow here")]
        [InlineData("bananas", "None", "Sorry, I didn't get that. Please try asking in a different way (intent was None)")]
        public async Task TaskSelector(string utterance, string intent, string invokedDialogResponse)
        {
            var mockLuisRecognizer = SimpleMockFactory.CreateMockLuisRecognizer<IRecognizer, FlightBooking>(
                new FlightBooking
                {
                    Intents = new Dictionary<FlightBooking.Intent, IntentScore>
                    {
                        { Enum.Parse<FlightBooking.Intent>(intent), new IntentScore() { Score = 1 } },
                    },
                    Entities = new FlightBooking._Entities(),
                });

            var sut = new MainDialog(_mockLogger.Object, mockLuisRecognizer.Object, _mockBookingDialog);
            var testClient = new DialogTestClient(Channels.Test, sut, middlewares: new[] { new XUnitDialogTestLogger(Output) });

            var reply = await testClient.SendActivityAsync<IMessageActivity>("hi");
            Assert.Equal("What can I help you with today?", reply.Text);

            reply = await testClient.SendActivityAsync<IMessageActivity>(utterance);
            Assert.Equal(invokedDialogResponse, reply.Text);

            reply = testClient.GetNextReply<IMessageActivity>();
            Assert.Equal("What else can I do for you?", reply.Text);
        }
    }
}
