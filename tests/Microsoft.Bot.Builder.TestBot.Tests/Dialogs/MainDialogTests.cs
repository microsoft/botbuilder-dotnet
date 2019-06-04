// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.CognitiveModels;
using Microsoft.BotBuilderSamples.Tests.Framework;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class MainDialogTests : DialogTestsBase
    {
        private readonly BookingDialog _mockBookingDialog;
        private readonly Mock<IRecognizer> _mockLuisRecognizer;

        public MainDialogTests(ITestOutputHelper output)
            : base(output)
        {
            _mockLuisRecognizer = new Mock<IRecognizer>();
            _mockBookingDialog = DialogUtils.CreateMockDialog<BookingDialog>().Object;
        }

        [Fact]
        public void DialogConstructor()
        {
            // TODO: check with the team if there's value in these types of test or if there's a better way of asserting the
            // dialog got composed properly.
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object, _mockLuisRecognizer.Object, _mockBookingDialog);

            Assert.Equal("MainDialog", sut.Id);
            Assert.IsType<TextPrompt>(sut.FindDialog("TextPrompt"));
            Assert.NotNull(sut.FindDialog("BookingDialog"));
            Assert.IsType<WaterfallDialog>(sut.FindDialog("WaterfallDialog"));
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("A", "", "")]
        [InlineData("", "B", "")]
        [InlineData("", "", "C")]
        [InlineData("A", "B", "")]
        [InlineData("A", "", "C")]
        [InlineData("", "B", "C")]
        public async Task ShowsMessageIfLuisNotConfigured(string luisAppId, string luisApiKey, string luisApiHostName)
        {
            // Arrange
            var luisMockConfig = new Mock<IConfiguration>();
            luisMockConfig.Setup(x => x["LuisAppId"]).Returns(luisAppId);
            luisMockConfig.Setup(x => x["LuisAPIKey"]).Returns(luisApiKey);
            luisMockConfig.Setup(x => x["LuisAPIHostName"]).Returns(luisApiHostName);

            var sut = new MainDialog(luisMockConfig.Object, MockLogger.Object, _mockLuisRecognizer.Object, _mockBookingDialog);
            var testClient = new DialogTestClient(sut, outputHelper: Output);

            // Act/Assert
            var reply = await testClient.SendAsync<IMessageActivity>("hi");
            Assert.Equal("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", reply.Text);

            reply = await testClient.GetNextReplyAsync<IMessageActivity>();
            Assert.Equal("What can I help you with today?", reply.Text);
        }

        [Fact]
        public async Task ShowsPromptIfLuisIsConfigured()
        {
            // Arrange
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object, _mockLuisRecognizer.Object, _mockBookingDialog);
            var testClient = new DialogTestClient(sut, outputHelper: Output);

            // Act/Assert
            var reply = await testClient.SendAsync<IMessageActivity>("hi");
            Assert.Equal("What can I help you with today?", reply.Text);
        }

        [Theory]
        [InlineData("I want to book a flight", "BookFlight", "BookingDialog mock invoked")]
        [InlineData("What's the weather like?", "GetWeather", "TODO: get weather flow here")]
        [InlineData("bananas", "None", "Sorry, I didn't get that. Please try asking in a different way (intent was None)")]
        public async Task TaskSelector(string utterance, string intent, string invokedDialogResponse)
        {
            _mockLuisRecognizer.SetupRecognizeAsync<FlightBooking>(
                new FlightBooking
                {
                    Intents = new Dictionary<FlightBooking.Intent, IntentScore>
                    {
                        { Enum.Parse<FlightBooking.Intent>(intent), new IntentScore() { Score = 1 } },
                    },
                });

            var sut = new MainDialog(MockConfig.Object, MockLogger.Object, _mockLuisRecognizer.Object, _mockBookingDialog);
            var testClient = new DialogTestClient(sut, outputHelper: Output);

            var reply = await testClient.SendAsync<IMessageActivity>("hi");
            Assert.Equal("What can I help you with today?", reply.Text);

            reply = await testClient.SendAsync<IMessageActivity>(utterance);
            Assert.Equal(invokedDialogResponse, reply.Text);

            reply = await testClient.GetNextReplyAsync<IMessageActivity>();
            Assert.Equal("What else can I do for you?", reply.Text);
        }
    }
}
