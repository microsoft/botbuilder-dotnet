using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    internal class RecognizerTelemetryUtils
    {
        public static readonly string CodeIntentText = "intent a1 b2";

        public static readonly string ColorIntentText = "I would like color red and orange";

        public static readonly string GreetingIntentTextEnUs = "howdy";

        public static async Task RecognizeIntentAndValidateTelemetry(string text, AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, int callCount)
        {
            var dc = TestUtils.CreateContext(text);
            var activity = dc.Context.Activity;
            var result = await recognizer.RecognizeAsync(dc, activity, CancellationToken.None);

            if (text == CodeIntentText)
            {
                ValidateCodeIntent(result);
            }
            
            if (text == ColorIntentText)
            {
                ValidateColorIntent(result);
            }

            ValidateTelemetry(recognizer, telemetryClient, dc, activity, callCount);
        }
        
        public static async Task RecognizeIntentAndValidateTelemetry_WithCustomActivity(string text, AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, int callCount)
        {
            var dc = TestUtils.CreateContext(string.Empty);
            var customActivity = Activity.CreateMessageActivity();
            customActivity.Text = text;
            customActivity.Locale = Culture.English;
            
            var result = await recognizer.RecognizeAsync(dc, (Activity)customActivity, CancellationToken.None);

            if (text == CodeIntentText)
            {
                ValidateCodeIntent(result);
            }

            if (text == ColorIntentText)
            {
                ValidateColorIntent(result);
            }

            if (text == GreetingIntentTextEnUs)
            {
                ValidateGreetingIntent(result);
            }

            ValidateTelemetry(recognizer, telemetryClient, dc, (Activity)customActivity, callCount);
        }

        public static void ValidateGreetingIntent(RecognizerResult result)
        {
            Assert.Single(result.Intents);
            Assert.Equal("Greeting", result.Intents.Select(i => i.Key).First());
        }

        /// <summary>
        /// Validates the colorIntent utterance "I would like colors red and orange".
        /// </summary>
        /// <param name="result">The <see cref="RecognizerResult"/>.</param>
        public static void ValidateColorIntent(RecognizerResult result)
        {
            Assert.Single(result.Intents);
            Assert.Equal("colorIntent", result.Intents.Select(i => i.Key).First());

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.NotNull(entities.color);
            Assert.Null(entities.code);
            Assert.Equal(2, entities.color.Count);
            Assert.Equal("red", (string)entities.color[0]);
            Assert.Equal("orange", (string)entities.color[1]);
        }

        /// <summary>
        /// Validates the codeIntent utterance "intent a1 b2".
        /// </summary>
        /// <param name="result">The <see cref="RecognizerResult"/>.</param>
        public static void ValidateCodeIntent(RecognizerResult result)
        {
            // intent assertions
            Assert.Single(result.Intents);
            Assert.Equal("codeIntent", result.Intents.Select(i => i.Key).First());

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.NotNull(entities.code);
            Assert.Null(entities.color);
            Assert.Equal(2, entities.code.Count);
            Assert.Equal("a1", (string)entities.code[0]);
            Assert.Equal("b2", (string)entities.code[1]);
        }

        public static void ValidateTelemetry(AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, int callCount)
        {
            var (logPersonalInfo, error) = recognizer.LogPersonalInformation.TryGetObject(dc.State);
            var telemetryProps = telemetryClient.Invocations[callCount - 1].Arguments[1];
            var eventName = GetEventName(recognizer.GetType().Name);

            telemetryClient.Verify(
                client => client.TrackEvent(
                    eventName,
                    It.Is<Dictionary<string, string>>(d => HasCorrectTelemetryProperties((IDictionary<string, string>)telemetryProps, activity, (bool)logPersonalInfo)),
                    null),
                Times.Exactly(callCount));
        }

        private static string GetEventName(string recognizerName)
        {
            return $"{recognizerName}Result";
        }

        private static bool HasCorrectTelemetryProperties(IDictionary<string, string> telemetryProperties, IActivity activity, bool logPersonalInformation)
        {
            var expectedProps = GetExpectedProps(activity, logPersonalInformation);

            if (expectedProps.Count == telemetryProperties.Count)
            {
                foreach (var entry in telemetryProperties)
                {
                    if (expectedProps.ContainsKey(entry.Key))
                    {
                        if (AreTelemetryPropertiesEqual(entry, telemetryProperties, logPersonalInformation, activity, expectedProps) == false)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private static Dictionary<string, string> GetExpectedProps(IActivity activity, bool logPersonalInformation)
        {
            var expectedProps = new Dictionary<string, string>();
            var text = activity.AsMessageActivity().Text;

            if (text == CodeIntentText)
            {
                expectedProps = GetCodeIntentProperties();
            }

            if (text == ColorIntentText)
            {
                expectedProps = GetColorIntentProperties();
            }

            if (text == GreetingIntentTextEnUs)
            {
                expectedProps = GetGreetingIntentProperties();
            }

            if (logPersonalInformation == true)
            {
                expectedProps.Add("Text", activity.AsMessageActivity().Text);
            }

            return expectedProps;
        }

        private static bool AreTelemetryPropertiesEqual(KeyValuePair<string, string> entry, IDictionary<string, string> telemetryProperties, bool logPersonalInformation, IActivity activity, IDictionary<string, string> expectedProps)
        {
            var areEqual = true;
            if (IsPiiProperty(entry.Key))
            {
                if (logPersonalInformation == false)
                {
                    areEqual = false;
                }

                if (!HasCorrectPiiValue(telemetryProperties))
                {
                    areEqual = false;
                }
            }
            else if (entry.Key == "Entities")
            {
                areEqual = HasValidEntities(activity, entry);
            }
            else
            {
                areEqual = entry.Value == expectedProps[entry.Key];
            }

            return areEqual;
        }

        private static bool IsPiiProperty(string telemetryProperty)
        {
            // In the future, should consider also including AlteredText
            return telemetryProperty == "Text";
        }

        private static bool HasCorrectPiiValue(IDictionary<string, string> telemetryProperties)
        {
            return telemetryProperties.ContainsKey("Text")
                && (telemetryProperties["Text"] == CodeIntentText 
                    || telemetryProperties["Text"] == ColorIntentText
                    || telemetryProperties["Text"] == GreetingIntentTextEnUs);
        }

        private static bool HasValidEntities(IActivity activity, KeyValuePair<string, string> entry)
        {
            var text = activity.AsMessageActivity().Text;
            var actualEntity = JsonConvert.DeserializeObject<Dictionary<string, object>>(entry.Value);

            if (text == CodeIntentText && !actualEntity.ContainsKey("code"))
            {
                return false;
            }

            if (text == ColorIntentText && !actualEntity.ContainsKey("color"))
            {
                return false;
            }

            if (text == GreetingIntentTextEnUs && actualEntity.Count != 0)
            {
                return false;
            }

            return true;
        }

        private static Dictionary<string, string> GetCodeIntentProperties()
        {
            return new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "codeIntent" },
                { "TopIntentScore", "Microsoft.Bot.Builder.IntentScore" },
                { "Intents", "{\"codeIntent\":{\"score\":1.0,\"pattern\":\"(?<code>[a-z][0-9])\"}}" },
                {
                    "Entities",
                    "{\r\n  \"code\": [\r\n    \"a1\",\r\n    \"b2\"\r\n  ],\r\n  \"$instance\": {\r\n    \"code\": [\r\n      {\r\n        \"startIndex\": 7,\r\n        \"endIndex\": 9,\r\n        \"score\": 1.0,\r\n        \"text\": \"a1\",\r\n        \"type\": \"code\",\r\n        \"resolution\": null\r\n      },\r\n      {\r\n        \"startIndex\": 10,\r\n        \"endIndex\": 12,\r\n        \"score\": 1.0,\r\n        \"text\": \"b2\",\r\n        \"type\": \"code\",\r\n        \"resolution\": null\r\n      }\r\n    ]\r\n  }\r\n}"
                },
                { "AdditionalProperties", null },
            };
        }

        private static Dictionary<string, string> GetColorIntentProperties()
        {
            return new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "colorIntent" },
                { "TopIntentScore", "Microsoft.Bot.Builder.IntentScore" },
                { "Intents", "{\"colorIntent\":{\"score\":1.0,\"pattern\":\"(?i)(color|colour)\"}}" },
                {
                    "Entities",
                    "{\r\n  \"color\": [\r\n    \"red\",\r\n    \"orange\"\r\n  ],\r\n  \"$instance\": {\r\n    \"color\": [\r\n      {\r\n        \"startIndex\": 19,\r\n        \"endIndex\": 23,\r\n        \"score\": 1.0,\r\n        \"text\": \"red\",\r\n        \"type\": \"color\",\r\n        \"resolution\": null\r\n      },\r\n      {\r\n        \"startIndex\": 27,\r\n        \"endIndex\": 34,\r\n        \"score\": 1.0,\r\n        \"text\": \"orange\",\r\n        \"type\": \"color\",\r\n        \"resolution\": null\r\n      }\r\n    ]\r\n  }\r\n}"
                },
                { "AdditionalProperties", null },
            };
        }

        private static Dictionary<string, string> GetGreetingIntentProperties()
        {
            return new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "Greeting" },
                { "TopIntentScore", "Microsoft.Bot.Builder.IntentScore" },
                { "Intents", "{\"Greeting\":{\"score\":1.0,\"pattern\":\"(?i)howdy\"}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", null }
            };
        }
    }
}
