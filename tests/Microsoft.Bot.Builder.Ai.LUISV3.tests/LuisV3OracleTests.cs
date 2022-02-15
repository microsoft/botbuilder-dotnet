﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace Microsoft.Bot.Builder.AI.LuisV3.Tests
{
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
    // The LUIS application used in these unit tests is in TestData/Contoso App.json
    public class LuisV3OracleTests : LuisSettings
    {
        // Access the checked-in oracles so that if they are changed you can compare the changes and easily modify them.
        private string testData = Path.Combine(new string[] { "..", "..", "..", "..", "Microsoft.Bot.Builder.AI.LUIS.Tests", "TestData" });

        private readonly Dictionary<string, IntentScore> _intents = new ()
        {
            { "Test", new IntentScore { Score = 0.2 } },
            { "Greeting", new IntentScore { Score = 0.4 } },
        };

        [Fact]
        public void LuisRecognizer_Timeout()
        {
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var expectedTimeout = 300;
            var optionsWithTimeout = new LuisRecognizerOptions()
            {
                Timeout = new TimeSpan(0, 0, 0, 0, expectedTimeout),
            };

            var recognizerWithTimeout = new LuisRecognizer(new LuisApplication(endpoint), optionsWithTimeout);
            Assert.NotNull(recognizerWithTimeout);
            Assert.Equal(expectedTimeout, LuisRecognizer.DefaultHttpClient.Timeout.Milliseconds);
        }

        [Fact]
        public void NullEndpoint()
        {
            // Arrange
            GetEnvironmentVars();
            var fieldInfo = typeof(LuisRecognizer).GetField("_application", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var myappNull = new LuisApplication(AppId, Key, null);
            var recognizerNull = new LuisRecognizer(myappNull, null);

            // Assert
            var app = (LuisApplication)fieldInfo.GetValue(recognizerNull);
            Assert.Equal("https://westus.api.cognitive.microsoft.com", app.Endpoint);
        }

        [Fact]
        public void EmptyEndpoint()
        {
            // Arrange
            GetEnvironmentVars();
            var fieldInfo = typeof(LuisRecognizer).GetField("_application", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var myappEmpty = new LuisApplication(AppId, Key, string.Empty);
            var recognizerEmpty = new LuisRecognizer(myappEmpty, null);

            // Assert
            var app = (LuisApplication)fieldInfo.GetValue(recognizerEmpty);
            Assert.Equal("https://westus.api.cognitive.microsoft.com", app.Endpoint);
        }

        [Fact]
        public void LuisRecognizer_NullLuisAppArg()
        {
            Assert.Throws<ArgumentNullException>(() => new LuisRecognizer(application: null));
        }

        [Fact]
        public async Task NullUtterance()
        {
            const string utterance = null;
            const string responsePath = "Patterns.json";   // The path is irrelevant in this case

            GetEnvironmentVars();
            var mockHttp = GetMockHttpClientHandlerObject(utterance, responsePath);
            var luisRecognizer = GetLuisRecognizer(mockHttp);
            var context = GetContext(utterance);
            var result = await luisRecognizer.RecognizeAsync(context, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Null(result.AlteredText);
            Assert.Equal(utterance, result.Text);
            Assert.NotNull(result.Intents);
            Assert.Single(result.Intents);
            Assert.NotNull(result.Intents[string.Empty]);
            Assert.Equal(result.GetTopScoringIntent(), (string.Empty, 1.0));
            Assert.NotNull(result.Entities);
            Assert.Empty(result.Entities);
        }

        public ITurnContext GetContext(string utterance)
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

        [Fact]
        public async Task TraceActivity()
        {
            const string utterance = @"My name is Emad";
            const string botResponse = @"Hi Emad";
            const string file = "TraceActivity.json";

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
                    #pragma warning disable CS0612 // Type or member is obsolete
                    Assert.Equal(LuisRecognizer.LuisTraceType, traceActivity.ValueType);
                    Assert.Equal(LuisRecognizer.LuisTraceLabel, traceActivity.Label);

                    var luisTraceInfo = JObject.FromObject(traceActivity.Value);
                    Assert.NotNull(luisTraceInfo);
                    Assert.NotNull(luisTraceInfo["recognizerResult"]);
                    Assert.NotNull(luisTraceInfo["luisResult"]);
                    Assert.NotNull(luisTraceInfo["luisOptions"]);
                    Assert.NotNull(luisTraceInfo["luisModel"]);

                    var recognizerResult = luisTraceInfo["recognizerResult"].ToObject<RecognizerResult>();
                    Assert.Equal(recognizerResult.Text, utterance);
                    Assert.NotNull(recognizerResult.Intents["SpecifyName"]);
                    Assert.Equal(luisTraceInfo["luisResult"]["query"], utterance);
                    Assert.Equal(luisTraceInfo["luisModel"]["ModelID"], AppId);
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
        public async Task DynamicLists() => await TestJson<RecognizerResult>("DynamicListsAndList.json");

        [Fact]
        public async Task ExternalEntitiesAndBuiltin() => await TestJson<RecognizerResult>("ExternalEntitiesAndBuiltin.json");

        [Fact]
        public async Task ExternalEntitiesAndComposite() => await TestJson<RecognizerResult>("ExternalEntitiesAndComposite.json");

        [Fact]
        public async Task ExternalEntitiesAndList() => await TestJson<RecognizerResult>("ExternalEntitiesAndList.json");

        [Fact]
        public async Task ExternalEntitiesAndRegex() => await TestJson<RecognizerResult>("ExternalEntitiesAndRegex.json");

        [Fact]
        public async Task ExternalEntitiesAndSimple() => await TestJson<RecognizerResult>("ExternalEntitiesAndSimple.json");

        [Fact]
        public async Task ExternalEntitiesAndSimpleOverride() => await TestJson<RecognizerResult>("ExternalEntitiesAndSimpleOverride.json");

        [Fact]
        public async Task GeoPeopleOrdinal() => await TestJson<RecognizerResult>("GeoPeopleOrdinal.json");

        [Fact]
        public async Task Minimal() => await TestJson<RecognizerResult>("Minimal.json");

        // TODO: This is disabled until the bug requiring instance data for geo is fixed.
        //[Fact]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public async Task MinimalWithGeo() => await TestJson<RecognizerResult>("MinimalWithGeo.json");
#pragma warning restore xUnit1013 // Public method should be marked as test

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
            RecognizerResult nullResults = null;
            Assert.Throws<ArgumentNullException>(() => LuisRecognizer.TopIntent(nullResults));
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
                ApplicationId = Guid.Empty.ToString(),
                Endpoint = "https://westus.api.cognitive.microsoft.com",
            };

            var clientHandler = new EmptyLuisResponseClientHandler();

            var recognizer = new LuisRecognizer(application, new LuisRecognizerOptions { HttpClient = clientHandler });

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

            // And that we added the bot.builder package details.
            var majorVersion = typeof(ConnectorClient).GetTypeInfo().Assembly.GetName().Version.Major;
            Assert.Contains($"Microsoft.Bot.Builder.AI.Luis/{majorVersion}", userAgent);
        }

        [Fact]
        public void Telemetry_Construction()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var fieldInfo = typeof(LuisRecognizer).GetField("_application", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var recognizer = new LuisRecognizer(new LuisApplication(endpoint));

            // Assert
            var app = (LuisApplication)fieldInfo.GetValue(recognizer);
            Assert.Equal("b31aeaf3-3511-495b-a07f-571fc873214b", app.ApplicationId);
            Assert.Equal("048ec46dc58e495482b0c447cfdbd291", app.EndpointKey);
            Assert.Equal("https://westus.api.cognitive.microsoft.com", app.Endpoint);
        }

        [Fact]
        public async Task Telemetry_OverrideOnLogAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
            var clientHandler = new EmptyLuisResponseClientHandler();
            var luisApp = new LuisApplication(endpoint);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var adapter = new NullAdapter();
            var options = new LuisRecognizerOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
                HttpClient = clientHandler,
            };

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "please book from May 5 to June 6",
                Recipient = new ChannelAccount(),           // to no where
                From = new ChannelAccount(),                // from no one
                Conversation = new ConversationAccount(),   // on no conversation
            };

            var turnContext = new TurnContext(adapter, activity);
            var recognizer = new LuisRecognizer(luisApp, options);

            // Act
            var additionalProperties = new Dictionary<string, string>
            {
                { "test", "testvalue" },
                { "foo", "foovalue" },
            };
            var result = await recognizer.RecognizeAsync(turnContext, additionalProperties).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(telemetryClient.Invocations);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("test"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["test"] == "testvalue");
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["foo"] == "foovalue");
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
        }

        [Fact]
        public async Task Telemetry_PiiLoggedAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
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
            var options = new LuisRecognizerOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = true,
                HttpClient = clientHandler,
            };
            var recognizer = new LuisRecognizer(luisApp, options);

            // Act
            var result = await recognizer.RecognizeAsync(turnContext).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(telemetryClient.Invocations);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).Count == 8);
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
        public async Task Telemetry_NoPiiLoggedAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
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
            var options = new LuisRecognizerOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
                HttpClient = clientHandler,
            };
            var recognizer = new LuisRecognizer(luisApp, options);

            // Act
            var result = await recognizer.RecognizeAsync(turnContext).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(telemetryClient.Invocations);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).Count == 7);
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
        public async Task Telemetry_OverrideOnDeriveAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
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

            var options = new LuisRecognizerOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
                HttpClient = clientHandler,
            };
            var recognizer = new TelemetryOverrideRecognizer(luisApp, options);

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
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"] == "myImportantValue");
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("test"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["test"] == "testvalue");
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["foo"] == "foovalue");
            Assert.Equal("MySecondEvent", telemetryClient.Invocations[1].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1]).ContainsKey("MyImportantProperty2"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1])["MyImportantProperty2"] == "myImportantValue2");
        }

        [Fact]
        public async Task Telemetry_OverrideFillAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
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

            var options = new LuisRecognizerOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
                HttpClient = clientHandler,
            };
            var recognizer = new OverrideFillRecognizer(luisApp, options);

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
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"] == "myImportantValue");
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("test"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["test"] == "testvalue");
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["foo"] == "foovalue");
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("moo"));
            Assert.Equal(3.14159, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["moo"]);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("boo"));
            Assert.Equal(2.11, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["boo"]);

            Assert.Equal("MySecondEvent", telemetryClient.Invocations[1].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1]).ContainsKey("MyImportantProperty2"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1])["MyImportantProperty2"] == "myImportantValue2");
        }

        [Fact]
        public async Task Telemetry_NoOverrideAsync()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
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
            var options = new LuisRecognizerOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
                HttpClient = clientHandler,
            };

            var recognizer = new LuisRecognizer(luisApp, options);

            // Act
            var result = await recognizer.RecognizeAsync(turnContext, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(telemetryClient.Invocations);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
        }

        [Fact]
        public async Task Telemetry_Convert()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
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
            var options = new LuisRecognizerOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
                HttpClient = clientHandler,
            };
            var recognizer = new LuisRecognizer(luisApp, options);

            // Act
            // Use a class the converts the Recognizer Result..
            var result = await recognizer.RecognizeAsync(turnContext, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(telemetryClient.Invocations);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("applicationId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intent"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("intentScore"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("entities"));
        }

        [Fact]
        public async Task Telemetry_ConvertParms()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";
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

            var options = new LuisRecognizerOptions
            {
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = false,
                HttpClient = clientHandler,
            };
            var recognizer = new LuisRecognizer(luisApp, options);

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

            var result = await recognizer.RecognizeAsync(turnContext, additionalProperties, additionalMetrics, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(telemetryClient.Invocations);
            Assert.Equal("LuisResult", telemetryClient.Invocations[0].Arguments[0].ToString());
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("test"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["test"] == "testvalue");
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["foo"] == "foovalue");
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
            var version = "v3";
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
#pragma warning disable CS0612 // Type or member is obsolete
                ? new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true, IncludeAPIResults = true }
#pragma warning restore CS0612 // Type or member is obsolete
                    : oracleOptions.ToObject<LuisPredictionOptions>();
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            response["options"] = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(options, settings));
            var luisRecognizer = GetLuisRecognizer(mockHttp, options);
            var typedResult = await luisRecognizer.RecognizeAsync<T>(context, CancellationToken.None);
            var typedJson = Utils.Json(typedResult, version, oracle);

            // Threshold is 0.0 so when hitting endpoint get exact and when mocking isn't needed.
            if (!Utils.WithinDelta(oracle, typedJson, Mock ? 0.0 : 0.01) && !JToken.DeepEquals(typedJson[version], oldResponse))
            {
                using (var writer = new StreamWriter(newPath))
                {
                    writer.Write(typedJson);
                }

                Assert.Equal(newPath, expectedPath);
            }
            else
            {
                File.Delete(expectedPath + ".new");
            }
        }

        private IRecognizer GetLuisRecognizer(MockedHttpClientHandler httpClientHandler, LuisPredictionOptions options = null)
        {
            var luisApp = new LuisApplication(AppId, Key, Endpoint);
            return new LuisRecognizer(luisApp, new LuisRecognizerOptions { HttpClient = httpClientHandler }, options);
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

        private MockedHttpClientHandler GetMockHttpClientHandler(string example, Stream response)
        {
            var mockMessageHandler = new MockHttpMessageHandler();
            mockMessageHandler.When(HttpMethod.Post, GetRequestUrl()).WithPartialContent(example).Respond("application/json", response);

            return new MockedHttpClientHandler(mockMessageHandler.ToHttpClient());
        }

        private string GetRequestUrl() => $"{Endpoint}/luis/prediction/v3.0/apps/{AppId}/*";

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
