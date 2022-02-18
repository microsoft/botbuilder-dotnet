﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.Luis.TestUtils;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete

    // The LUIS application used in these unit tests is in TestData/Contoso App.json
    public class LuisOracleTests : LuisSettings
    {
        // Access the checked-in oracles so that if they are changed you can compare the changes and easily modify them.
        private readonly string testData = Path.Combine(new string[] { "..", "..", "..", "TestData" });

        private readonly Dictionary<string, IntentScore> _intents = new Dictionary<string, IntentScore>()
        {
            { "Test", new IntentScore { Score = 0.2 } },
            { "Greeting", new IntentScore { Score = 0.4 } },
        };

        [Fact]
        public void LuisRecognizerConstruction()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            GetEnvironmentVars();
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var fieldInfo = typeof(LuisRecognizer).GetField("_luisRecognizerOptions", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var recognizer = new LuisRecognizer(new LuisRecognizerOptionsV2(new LuisApplication(endpoint)));

            // Assert
            var app = (LuisRecognizerOptions)fieldInfo.GetValue(recognizer);
            Assert.Equal("b31aeaf3-3511-495b-a07f-571fc873214b", app.Application.ApplicationId);
            Assert.Equal("048ec46dc58e495482b0c447cfdbd291", app.Application.EndpointKey);
            Assert.Equal("https://westus.api.cognitive.microsoft.com", app.Application.Endpoint);
        }

        [Fact]
        public void LuisRecognizer_Timeout()
        {
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";

            var expectedTimeout = 300;

            var opts = new LuisRecognizerOptionsV2(new LuisApplication(endpoint))
            {
                Timeout = 300,
            };

            var recognizerWithTimeout = new LuisRecognizer(opts);
            Assert.NotNull(recognizerWithTimeout);
            Assert.Equal(expectedTimeout, recognizerWithTimeout.HttpClient.Timeout.Milliseconds);
        }

        [Fact]
        public void NullEndpoint()
        {
            // Arrange
            GetEnvironmentVars();
            var fieldInfo = typeof(LuisRecognizer).GetField("_luisRecognizerOptions", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var myappNull = new LuisApplication(AppId, Key, null);
            var recognizerNull = new LuisRecognizer(new LuisRecognizerOptionsV2(myappNull), null);

            // Assert
            var app = (LuisRecognizerOptions)fieldInfo.GetValue(recognizerNull);
            Assert.Equal("https://westus.api.cognitive.microsoft.com", app.Application.Endpoint);
        }

        [Fact]
        public void EmptyEndpoint()
        {
            // Arrange
            GetEnvironmentVars();
            var fieldInfo = typeof(LuisRecognizer).GetField("_luisRecognizerOptions", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var myappEmpty = new LuisApplication(AppId, Key, string.Empty);

            var recognizerEmpty = new LuisRecognizer(new LuisRecognizerOptionsV2(myappEmpty), null);

            // Assert
            var app = (LuisRecognizerOptions)fieldInfo.GetValue(recognizerEmpty);
            Assert.Equal("https://westus.api.cognitive.microsoft.com", app.Application.Endpoint);
        }

        [Fact]
        public void LuisRecognizer_NullLuisAppArg()
        {
            Assert.Throws<ArgumentNullException>(() => new LuisRecognizer(new LuisRecognizerOptionsV2(application: null)));
        }

        [Fact]
        public async Task NullUtterance()
        {
            const string utterance = null;
            const string responsePath = "Patterns.json";   // The path is irrelevant in this case

            GetEnvironmentVars();
            var mockHttp = GetMockHttpClientHandlerObject(utterance, responsePath);
            var luisRecognizer = GetLuisRecognizer(mockHttp, verbose: true);
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Null(result.AlteredText);
            Assert.Equal(utterance, result.Text);
            Assert.NotNull(result.Intents);
            Assert.Empty(result.Intents);
            Assert.NotNull(result.Entities);
            Assert.Empty(result.Entities);
        }

        [Fact]
        public async Task UtteranceWithoutTurnContext()
        {
            const string utterance = "email about something wicked this way comes from bart simpson and also kb435";
            const string responsePath = "Patterns.json";
            var expectedPath = GetFilePath(responsePath);
            JObject oracle;
            using (var expectedJsonReader = new JsonTextReader(new StreamReader(expectedPath)))
            {
                oracle = (JObject)await JToken.ReadFromAsync(expectedJsonReader);
            }

            var mockResponse = oracle["v2"]["response"];

            GetEnvironmentVars();

            var mockHttp = GetMockHttpClientHandlerObject(utterance, new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mockResponse))));
            var options = new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true };
            var luisRecognizer = GetLuisRecognizer(mockHttp, false, options) as LuisRecognizer;
            var result = await luisRecognizer.RecognizeAsync(utterance);

            Assert.NotNull(result);
            Assert.Null(result.AlteredText);
            Assert.Equal(utterance, result.Text);
            Assert.NotNull(result.Intents);
            Assert.NotNull(result.Entities);
        }

        [Fact]
        public async Task V1DatetimeResolution()
        {
            const string utterance = "at 4";
            const string responsePath = "V1DatetimeResolution.json";

            GetEnvironmentVars();
            var mockHttp = GetMockHttpClientHandler(utterance, responsePath);
            var luisRecognizer = GetLuisRecognizer(mockHttp, true, new LuisPredictionOptions { IncludeAllIntents = true });
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.NotNull(result.Entities["datetime_time"]);
            Assert.Single(result.Entities["datetime_time"]);
            Assert.Equal("ampm", (string)result.Entities["datetime_time"][0]["comment"]);
            Assert.Equal("T04", (string)result.Entities["datetime_time"][0]["time"]);
            Assert.Single(result.Entities["$instance"]["datetime_time"]);
        }

        [Fact]
        public async Task TraceActivity()
        {
            const string utterance = @"My name is Emad";
            const string botResponse = @"Hi Emad";
            const string file = "TraceActivity.json";

            GetEnvironmentVars();
            var adapter = new TestAdapter(Channels.Test, true);
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                if (context.Activity.Text == utterance)
                {
                    await TestJson<RecognizerResult>(file, context);
                    await context.SendActivityAsync(botResponse);
                }
            })
                .Test(
                utterance,
                activity =>
                {
                    var traceActivity = activity as ITraceActivity;
                    Assert.NotNull(traceActivity);
                    Assert.Equal(LuisRecognizer.LuisTraceType, traceActivity.ValueType);
                    Assert.Equal(LuisRecognizer.LuisTraceLabel, traceActivity.Label);

                    var luisTraceInfo = JObject.FromObject(traceActivity.Value);
                    Assert.NotNull(luisTraceInfo);
                    Assert.NotNull(luisTraceInfo["recognizerResult"]);
                    Assert.NotNull(luisTraceInfo["luisResult"]);
                    Assert.NotNull(luisTraceInfo["luisOptions"]);
                    Assert.NotNull(luisTraceInfo["luisModel"]);

                    var recognizerResult = luisTraceInfo["recognizerResult"].ToObject<RecognizerResult>();
                    Assert.Equal(utterance, recognizerResult.Text);
                    Assert.NotNull(recognizerResult.Intents["SpecifyName"]);
                    Assert.Equal(utterance, luisTraceInfo["luisResult"]["query"]);
                    Assert.Equal(AppId, luisTraceInfo["luisModel"]["ModelID"]);
                    Assert.Equal(default(bool?), luisTraceInfo["luisOptions"]["Staging"]);
                })
                .Send(utterance)
                .AssertReply(botResponse, "passthrough")
                .StartTestAsync();
        }

        [Fact]
        public async Task Composite1() => await TestJson<RecognizerResult>("Composite1.json");

        [Fact]
        public async Task Composite2() => await TestJson<RecognizerResult>("Composite2.json");

        [Fact]
        public async Task Composite3() => await TestJson<RecognizerResult>("Composite3.json");

        [Fact]
        public async Task GeoPeopleOrdinal() => await TestJson<RecognizerResult>("GeoPeopleOrdinal.json");

        [Fact]
        public async Task Minimal() => await TestJson<RecognizerResult>("Minimal.json");

        [Fact]
        public async Task MinimalWithGeo() => await TestJson<RecognizerResult>("MinimalWithGeo.json");

        [Fact]
        public async Task PrebuiltDomains() => await TestJson<RecognizerResult>("Prebuilt.json");

        [Fact]
        public async Task Patterns() => await TestJson<RecognizerResult>("Patterns.json");

        [Fact]
        public async Task Roles() => await TestJson<RecognizerResult>("roles.json");

        [Fact]
        public async Task TypedEntities() => await TestJson<Contoso_App>("Typed.json");

        [Fact]
        public async Task TypedPrebuiltDomains() => await TestJson<Contoso_App>("TypedPrebuilt.json");

        [Fact]
        public void TopIntentReturnsTopIntent()
        {
            var results = new RecognizerResult();
            _intents.ToList().ForEach(results.Intents.Add);

            var greetingIntent = LuisRecognizer.TopIntent(results);
            Assert.Equal("Greeting", greetingIntent);
        }

        [Fact]
        public void TopIntentReturnsDefaultIntentIfMinScoreIsHigher()
        {
            var results = new RecognizerResult();
            _intents.ToList().ForEach(results.Intents.Add);

            var defaultIntent = LuisRecognizer.TopIntent(results, minScore: 0.5);
            Assert.Equal("None", defaultIntent);
        }

        [Fact]
        public void TopIntentReturnsDefaultIntentIfProvided()
        {
            var results = new RecognizerResult();
            _intents.ToList().ForEach(results.Intents.Add);

            var defaultIntent = LuisRecognizer.TopIntent(results, "Test2", 0.5);
            Assert.Equal("Test2", defaultIntent);
        }

        [Fact]
        public void TopIntentThrowsArgumentNullExceptionIfResultsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                RecognizerResult nullResults = null;
                LuisRecognizer.TopIntent(nullResults);
            });
        }

        [Fact]
        public void TopIntentReturnsTopIntentIfScoreEqualsMinScore()
        {
            var results = new RecognizerResult();
            _intents.ToList().ForEach(results.Intents.Add);

            var defaultIntent = LuisRecognizer.TopIntent(results, minScore: 0.4);
            Assert.Equal("Greeting", defaultIntent);
        }

        [Fact]
        public void UserAgentContainsProductVersion()
        {
            var application = new LuisApplication
            {
                EndpointKey = "this-is-not-a-key",
                ApplicationId = "this-is-not-an-application-id",
                Endpoint = "https://westus.api.cognitive.microsoft.com",
            };

            var clientHandler = new EmptyLuisResponseClientHandler();

            var recognizer = new LuisRecognizer(new LuisRecognizerOptionsV2(application), clientHandler: clientHandler);

            var adapter = new NullAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),   // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);

            var recognizerResult = recognizer.RecognizeAsync(turnContext, CancellationToken.None).Result;
            Assert.NotNull(recognizerResult);

            var userAgent = clientHandler.UserAgent;

            // Verify we didn't unintentionally stamp on the user-agent from the client.
            Assert.Contains("Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.LUISRuntimeClient", userAgent);

            // And that we added the bot.builder package details.
            var majorVersion = typeof(ConnectorClient).GetTypeInfo().Assembly.GetName().Version.Major;
            Assert.Contains($"microsoft.bot.builder.ai.luis/{majorVersion}", userAgent.ToLower());
        }

        [Fact]
        public void Telemetry_Construction()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var fieldInfo = typeof(LuisRecognizer).GetField("_luisRecognizerOptions", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var recognizer = new LuisRecognizer(new LuisRecognizerOptionsV2(new LuisApplication(endpoint)));

            // Assert
            var app = (LuisRecognizerOptions)fieldInfo.GetValue(recognizer);
            Assert.Equal("b31aeaf3-3511-495b-a07f-571fc873214b", app.Application.ApplicationId);
            Assert.Equal("048ec46dc58e495482b0c447cfdbd291", app.Application.EndpointKey);
            Assert.Equal("https://westus.api.cognitive.microsoft.com", app.Application.Endpoint);
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
        public async Task Telemetry_OverrideOnLogAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),   // on no conversation
            };

            var opts = new LuisRecognizerOptionsV2(luisApp)
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
            };

            var turnContext = new TurnContext(adapter, activity);
            var recognizer = new LuisRecognizer(opts, clientHandler);

            // Act
            var additionalProperties = new Dictionary<string, string>
            {
                { "test", "testvalue" },
                { "foo", "foovalue" },
            };
            var result = await recognizer.RecognizeAsync(turnContext, additionalProperties).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("test"));
            Assert.Equal("testvalue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["test"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.Equal("foovalue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["foo"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
        public async Task Telemetry_PiiLoggedAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),   // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);

            var opts = new LuisRecognizerOptionsV2(luisApp)
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = true,
            };
            var recognizer = new LuisRecognizer(opts, clientHandler);

            // Act
            var result = await recognizer.RecognizeAsync(turnContext, null).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.Equal(8, ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).Count);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent2"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore2"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
        public async Task Telemetry_NoPiiLoggedAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),   // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);
            var options = new LuisRecognizerOptionsV2(luisApp)
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
            };
            var recognizer = new LuisRecognizer(options, clientHandler);

            // Act
            var result = await recognizer.RecognizeAsync(turnContext, null).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.Equal(7, ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).Count);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent2"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore2"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
            Assert.False(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
        public async Task Telemetry_OverrideOnDeriveAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),    // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);

            var options = new LuisPredictionOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
            };
            var recognizer = new TelemetryOverrideRecognizer(luisApp, options, false, false, clientHandler);

            var additionalProperties = new Dictionary<string, string>
            {
                { "test", "testvalue" },
                { "foo", "foovalue" },
            };
            var result = await recognizer.RecognizeAsync(turnContext, additionalProperties).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.Equal("myImportantValue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("test"));
            Assert.Equal("testvalue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["test"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.Equal("foovalue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["foo"]);
            Assert.Equal("MySecondEvent", telemetryClient.Invocations[1].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1]).ContainsKey("MyImportantProperty2"));
            Assert.Equal("myImportantValue2", ((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1])["MyImportantProperty2"]);
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
        public async Task Telemetry_OverrideFillAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),    // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);

            var options = new LuisPredictionOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
            };
            var recognizer = new OverrideFillRecognizer(luisApp, options, false, false, clientHandler);

            var additionalProperties = new Dictionary<string, string>
            {
                { "test", "testvalue" },
                { "foo", "foovalue" },
            };
            var additionalMetrics = new Dictionary<string, double>
            {
                { "moo", 3.14159 },
                { "boo", 2.11 },
            };

            var result = await recognizer.RecognizeAsync(turnContext, additionalProperties, additionalMetrics).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.Equal("myImportantValue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("test"));
            Assert.Equal("testvalue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["test"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.Equal("foovalue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["foo"]);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("moo"));
            Assert.Equal(3.14159, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["moo"]);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("boo"));
            Assert.Equal(2.11, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["boo"]);
            Assert.Equal("MySecondEvent", telemetryClient.Invocations[1].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1]).ContainsKey("MyImportantProperty2"));
            Assert.Equal("myImportantValue2", ((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1])["MyImportantProperty2"]);
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
        public async Task Telemetry_NoOverrideAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),    // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);

            var opts = new LuisRecognizerOptionsV2(luisApp)
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
            };

            var recognizer = new LuisRecognizer(opts, clientHandler);

            // Act
            var result = await recognizer.RecognizeAsync(turnContext, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
        public async Task Telemetry_Convert()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),    // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);
            var options = new LuisRecognizerOptionsV2(luisApp)
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
            };
            var recognizer = new LuisRecognizer(options, clientHandler);

            // Act
            // Use a class the converts the Recognizer Result..
            var result = await recognizer.RecognizeAsync<TelemetryConvertResult>(turnContext, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
        public async Task Telemetry_ConvertParms()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),    // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);

            var options = new LuisRecognizerOptionsV2(luisApp)
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
            };
            var recognizer = new LuisRecognizer(options, clientHandler);

            // Act
            var additionalProperties = new Dictionary<string, string>
            {
                { "test", "testvalue" },
                { "foo", "foovalue" },
            };
            var additionalMetrics = new Dictionary<string, double>
            {
                { "moo", 3.14159 },
                { "luis", 1.0001 },
            };

            var result = await recognizer.RecognizeAsync<TelemetryConvertResult>(turnContext, additionalProperties, additionalMetrics, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("test"));
            Assert.Equal("testvalue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["test"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.Equal("foovalue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["foo"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("moo"));
            Assert.Equal(3.14159, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["moo"]);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("luis"));
            Assert.Equal(1.0001, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["luis"]);
        }

        // To create a file to test:
        // 1) Create a <name>.json file with an object { text:<query> } in it.
        // 2) Run this test which will fail and generate a <name>.json.new file.
        // 3) Check the .new file and if correct, replace the original .json file with it.
        // The version parameter controls where in the expected json the luisResult is put.  This allows multiple endpoint responses like from
        // LUIS V2 and V3 endpoints.  You should run V3 first since it sometimes adds more information that V2.
        // NOTE: The same oracle files are shared between Luis and LuisPreview in order to ensure the mapping is the same.
        private async Task TestJson<T>(string file, ITurnContext turnContext = null)
            where T : IRecognizerConvert, new()
        {
            GetEnvironmentVars();
            var version = "v2";
            var expectedPath = GetFilePath(file);
            JObject oracle;
            using (var expectedJsonReader = new JsonTextReader(new StreamReader(expectedPath)))
            {
                oracle = (JObject)await JToken.ReadFromAsync(expectedJsonReader);
            }

            if (oracle[version] == null)
            {
                oracle[version] = new JObject();
            }

            var oldResponse = oracle[version].DeepClone();
            var newPath = expectedPath + ".new";
            var query = oracle["text"].ToString();
            var context = turnContext ?? GetContext(query);
            var response = oracle[version];
            var mockResponse = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response?["response"])));
            var mockHttp = GetMockHttpClientHandlerObject((string)oracle["text"], mockResponse);
            var oracleOptions = response["options"];
            var options = (oracleOptions == null || oracleOptions.Type == JTokenType.Null)
                ? new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true }
                    : oracleOptions.ToObject<LuisPredictionOptions>();
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            response["options"] = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(options, settings));
            var luisRecognizer = GetLuisRecognizer(mockHttp, true, options);
            var typedResult = await luisRecognizer.RecognizeAsync<T>(context, CancellationToken.None);
            var typedJson = Utils.Json(typedResult, version, oracle);
            if (!Utils.WithinDelta(oracle, typedJson, 0.01) || !JToken.DeepEquals(typedJson[version], oldResponse))
            {
                using (var writer = new StreamWriter(newPath))
                {
                    writer.Write(typedJson);
                }

                throw new XunitException($"Returned JSON in {newPath} != expected JSON in {expectedPath}");
            }
            else
            {
                File.Delete(expectedPath + ".new");
            }
        }

        private ITurnContext GetContext(string utterance)
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

        private IRecognizer GetLuisRecognizer(MockedHttpClientHandler httpClientHandler, bool verbose = false, LuisPredictionOptions options = null)
        {
            var luisApp = new LuisApplication(AppId, Key, Endpoint);
            return new LuisRecognizer(luisApp, options, verbose, httpClientHandler);
        }

        private MockedHttpClientHandler GetMockHttpClientHandlerObject(string example, string responsePath)
        {
            var response = GetResponse(responsePath);
            return GetMockHttpClientHandlerObject(example, response);
        }

        private MockedHttpClientHandler GetMockHttpClientHandlerObject(string example, Stream response)
        {
            if (Mock)
            {
                return GetMockHttpClientHandler(example, response);
            }
            else
            {
                return null;
            }
        }

        private MockedHttpClientHandler GetMockHttpClientHandler(string example, string responsePath)
            => GetMockHttpClientHandler(example, GetResponse(responsePath));

        private MockedHttpClientHandler GetMockHttpClientHandler(string example, Stream response)
        {
            var mockMessageHandler = new MockHttpMessageHandler();
            mockMessageHandler.When(GetRequestUrl()).WithPartialContent(example)
                .Respond("application/json", response);

            return new MockedHttpClientHandler(mockMessageHandler.ToHttpClient());
        }

        private string GetRequestUrl() => $"{Endpoint}/luis/v2.0/apps/{AppId}";

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(testData, fileName);
            return File.OpenRead(path);
        }

        private string GetFilePath(string fileName)
        {
            var path = Path.Combine(testData, fileName);
            return path;
        }
    }
}
