// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
#pragma warning disable CS0612 // Type or member is obsolete
    public class LuisRecognizerTests
    {
        private readonly LuisApplication _luisApp;
        private readonly EmptyLuisResponseClientHandler _mockHttpClientHandler;

        public LuisRecognizerTests()
        {
            _luisApp = new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "https://someluisendpoint");
            _mockHttpClientHandler = new EmptyLuisResponseClientHandler();
        }

        [Theory]
        [InlineData(false, 42.0, false, "Fake2806-EC0A-472D-95B7-A7132D159E03", false, false)]
        [InlineData(true, 42.0, false, "Fake2806-EC0A-472D-95B7-A7132D159E03", false, false)]
        [InlineData(false, 42.0, true, "Fake2806-EC0A-472D-95B7-A7132D159E03", false, false)]
        [InlineData(false, 42.0, false, "Fake2806-EC0A-472D-95B7-A7132D159E03", true, false)]
        [InlineData(false, 42.0, false, "Fake2806-EC0A-472D-95B7-A7132D159E03", false, true)]
        [InlineData(null, 42.0, false, "Fake2806-EC0A-472D-95B7-A7132D159E03", false, false)]
        [InlineData(false, null, false, "Fake2806-EC0A-472D-95B7-A7132D159E03", false, false)]
        [InlineData(false, 42.0, null, "Fake2806-EC0A-472D-95B7-A7132D159E03", false, false)]
        [InlineData(false, 42.0, false, null, false, false)]
        [InlineData(false, 42.0, false, "Fake2806-EC0A-472D-95B7-A7132D159E03", null, false)]
        [InlineData(false, 42.0, false, "Fake2806-EC0A-472D-95B7-A7132D159E03", false, null)]
        [InlineData(null, null, null, null, null, null)]
        public async Task LuisPredictionOptionsAreUsedInTheRequest(bool? includeAllIntents, double? timezoneOffset, bool? spellCheck, string bingSpellCheckSubscriptionKey, bool? log, bool? staging)
        {
            // Arrange
            var expectedOptions = new LuisPredictionOptions()
            {
                IncludeAllIntents = includeAllIntents,
                TimezoneOffset = timezoneOffset,
                SpellCheck = spellCheck,
                BingSpellCheckSubscriptionKey = bingSpellCheckSubscriptionKey,
                Log = log ?? true,
                Staging = staging,
            };

            var opts = new LuisRecognizerOptionsV2(_luisApp)
            {
                PredictionOptions = expectedOptions,
            };

            var sut = new LuisRecognizer(opts, clientHandler: _mockHttpClientHandler);

            // Act
            await sut.RecognizeAsync(BuildTurnContextForUtterance("hi"), CancellationToken.None);

            // Assert
            AssertLuisRequest(_mockHttpClientHandler.RequestMessage, expectedOptions);
        }

        [Theory]
        [InlineData(false, null, null, null, null, null)]
        [InlineData(null, 55.0, null, null, null, null)]
        [InlineData(null, null, false, null, null, null)]
        [InlineData(null, null, null, "Override-EC0A-472D-95B7-A7132D159E03", null, null)]
        [InlineData(null, null, null, null, true, null)]
        [InlineData(null, null, null, null, null, false)]
        [InlineData(null, null, null, null, null, null)]
        public async Task ShouldOverridePredictionOptionsIfProvided(bool? includeAllIntents, double? timezoneOffset, bool? spellCheck, string bingSpellCheckSubscriptionKey, bool? log, bool? staging)
        {
            // Arrange
            // Initialize options with non default values so we can assert they have been overriden.
            var constructorOptions = new LuisPredictionOptions()
            {
                IncludeAllIntents = true,
                TimezoneOffset = 42,
                SpellCheck = true,
                BingSpellCheckSubscriptionKey = "Fake2806-EC0A-472D-95B7-A7132D159E03",
                Log = false,
                Staging = true,
            };

            // Create overriden options for call
            var overridenOptions = new LuisPredictionOptions()
            {
                IncludeAllIntents = includeAllIntents,
                TimezoneOffset = timezoneOffset,
                SpellCheck = spellCheck,
                BingSpellCheckSubscriptionKey = bingSpellCheckSubscriptionKey,
                Log = log,
                Staging = staging,
            };

#pragma warning disable CS0618 // Type or member is obsolete

            // Create combined options for assertion taking the test case value if not null or the constructor value if not null.
            var expectedOptions = new LuisPredictionOptions()
            {
                IncludeAllIntents = includeAllIntents ?? constructorOptions.IncludeAllIntents,
                TimezoneOffset = timezoneOffset ?? constructorOptions.TimezoneOffset,
                SpellCheck = spellCheck ?? constructorOptions.SpellCheck,
                BingSpellCheckSubscriptionKey = bingSpellCheckSubscriptionKey ?? constructorOptions.BingSpellCheckSubscriptionKey,
                Log = log ?? constructorOptions.Log,
                Staging = staging ?? constructorOptions.Staging,
                LogPersonalInformation = constructorOptions.LogPersonalInformation,
            };

            var opts = new LuisRecognizerOptionsV2(_luisApp)
            {
                PredictionOptions = constructorOptions,
                TelemetryClient = constructorOptions.TelemetryClient,
            };

            // var sut = new LuisRecognizer(_luisApp, constructorOptions, clientHandler: _mockHttpClientHandler);
            var sut = new LuisRecognizer(opts, clientHandler: _mockHttpClientHandler);

            // Act/Assert RecognizeAsync override
            await sut.RecognizeAsync(BuildTurnContextForUtterance("hi"), overridenOptions, CancellationToken.None);
            AssertLuisRequest(_mockHttpClientHandler.RequestMessage, expectedOptions);

            // these values can't be overriden and should stay unchanged.
            Console.WriteLine(constructorOptions.TelemetryClient == sut.TelemetryClient);
            Assert.Equal(constructorOptions.TelemetryClient, sut.TelemetryClient);
            Assert.Equal(constructorOptions.LogPersonalInformation, sut.LogPersonalInformation);

            // Act/Assert RecognizeAsync<T> override
            await sut.RecognizeAsync<Contoso_App>(BuildTurnContextForUtterance("hi"), overridenOptions, CancellationToken.None);
            AssertLuisRequest(_mockHttpClientHandler.RequestMessage, expectedOptions);

            // these values can't be overriden and should stay unchanged.
            Assert.Equal(constructorOptions.TelemetryClient, sut.TelemetryClient);
            Assert.Equal(constructorOptions.LogPersonalInformation, sut.LogPersonalInformation);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Theory]
        [InlineData(false, null, null, null, null, null)]
        [InlineData(null, 55.0, null, null, null, null)]
        [InlineData(null, null, false, null, null, null)]
        [InlineData(null, null, null, "Override-EC0A-472D-95B7-A7132D159E03", null, null)]
        [InlineData(null, null, null, null, true, null)]
        [InlineData(null, null, null, null, null, false)]
        [InlineData(null, null, null, null, null, null)]
        public async Task ShouldOverrideRecognizerOptionsIfProvided(bool? includeAllIntents, double? timezoneOffset, bool? spellCheck, string bingSpellCheckSubscriptionKey, bool? log, bool? staging)
        {
            // Arrange
            // Initialize options with non default values so we can assert they have been overriden.
            var constructorOptions = new LuisPredictionOptions()
            {
                IncludeAllIntents = true,
                TimezoneOffset = 42.0,
                SpellCheck = true,
                BingSpellCheckSubscriptionKey = "Fake2806-EC0A-472D-95B7-A7132D159E03",
                Log = false,
                Staging = true,
            };

            // Create overriden options for call
            var overridenOptions = new LuisRecognizerOptionsV2(_luisApp)
            {
                PredictionOptions = new LuisPredictionOptions()
                {
                    IncludeAllIntents = includeAllIntents,
                    TimezoneOffset = timezoneOffset,
                    SpellCheck = spellCheck,
                    BingSpellCheckSubscriptionKey = bingSpellCheckSubscriptionKey,
                    Log = log,
                    Staging = staging
                }
            };

            // Create combined options for assertion taking the test case value if not null or the constructor value if not null.
            var expectedOptions = new LuisPredictionOptions()
            {
                IncludeAllIntents = includeAllIntents,
                TimezoneOffset = timezoneOffset,
                SpellCheck = spellCheck,
                BingSpellCheckSubscriptionKey = bingSpellCheckSubscriptionKey,
                Log = log ?? true,
                Staging = staging,
            };

#pragma warning disable CS0618 // Type or member is obsolete
            var opts = new LuisRecognizerOptionsV2(_luisApp)
            {
                PredictionOptions = constructorOptions,
                TelemetryClient = constructorOptions.TelemetryClient,
            };

            // var sut = new LuisRecognizer(_luisApp, constructorOptions, clientHandler: _mockHttpClientHandler);
            var sut = new LuisRecognizer(opts, clientHandler: _mockHttpClientHandler);

            // Act/Assert RecognizeAsync override
            await sut.RecognizeAsync(BuildTurnContextForUtterance("hi"), overridenOptions, CancellationToken.None);
            AssertLuisRequest(_mockHttpClientHandler.RequestMessage, expectedOptions);

#pragma warning disable CS0618 // Type or member is obsolete

            // these values can't be overriden and should stay unchanged.
            Console.WriteLine(constructorOptions.TelemetryClient == sut.TelemetryClient);
            Assert.Equal(constructorOptions.TelemetryClient, sut.TelemetryClient);
            Assert.Equal(constructorOptions.LogPersonalInformation, sut.LogPersonalInformation);

            // Act/Assert RecognizeAsync<T> override
            await sut.RecognizeAsync<Contoso_App>(BuildTurnContextForUtterance("hi"), overridenOptions, CancellationToken.None);
            AssertLuisRequest(_mockHttpClientHandler.RequestMessage, expectedOptions);

            // these values can't be overriden and should stay unchanged.
            Assert.Equal(constructorOptions.TelemetryClient, sut.TelemetryClient);
            Assert.Equal(constructorOptions.LogPersonalInformation, sut.LogPersonalInformation);
        }

        private static void AssertLuisRequest(HttpRequestMessage httpRequestForLuis, LuisPredictionOptions expectedOptions)
        {
            var queryStringParameters = HttpUtility.ParseQueryString(httpRequestForLuis.RequestUri.Query);
            Assert.Equal(expectedOptions.BingSpellCheckSubscriptionKey?.ToString(CultureInfo.InvariantCulture), queryStringParameters["bing-spell-check-subscription-key"]);
            Assert.Equal(expectedOptions.SpellCheck?.ToString(CultureInfo.InvariantCulture).ToLower(), queryStringParameters["spellCheck"]);
            Assert.Equal(expectedOptions.IncludeAllIntents?.ToString(CultureInfo.InvariantCulture).ToLower(), queryStringParameters["verbose"]);
            Assert.Equal(expectedOptions.Staging?.ToString(CultureInfo.InvariantCulture).ToLower(), queryStringParameters["staging"]);
            Assert.Equal(expectedOptions.Log?.ToString(CultureInfo.InvariantCulture).ToLower(), queryStringParameters["log"]);
        }

        private static TurnContext BuildTurnContextForUtterance(string utterance)
        {
            var testAdapter = new TestAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = utterance,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            return new TurnContext(testAdapter, activity);
        }
    }
}
