using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Moq;
using Newtonsoft.Json;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.IntentValidations;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.TestTelemetryProperties;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    internal class RecognizerTelemetryUtils
    {
        internal static readonly string CodeIntentText = "intent a1 b2";

        internal static readonly string ColorIntentText = "I would like color red and orange";

        internal static readonly string GreetingIntentTextEnUs = "howdy";

        internal static readonly string CrossTrainText = "criss-cross applesauce";

        internal static readonly string X = "x";

        private static readonly Dictionary<string, Func<Dictionary<string, string>>> ExpectedProperties = new Dictionary<string, Func<Dictionary<string, string>>>()
        {
            { CodeIntentText, GetCodeIntentProperties },
            { ColorIntentText, GetColorIntentProperties },
            { GreetingIntentTextEnUs, GetGreetingIntentProperties },
            { CrossTrainText, GetChooseIntentProperties },
            { X, GetXIntentProperties }
        };

        private static readonly Dictionary<string, Action<RecognizerResult>> ValidateIntent = new Dictionary<string, Action<RecognizerResult>>()
        {
            { CodeIntentText, ValidateCodeIntent },
            { ColorIntentText, ValidateColorIntent },
            { GreetingIntentTextEnUs, ValidateGreetingIntent },
            { CrossTrainText, ValidateChooseIntent },
            { X, ValidateXIntent }
        };

        internal static async Task RecognizeIntentAndValidateTelemetry(string text, AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, int callCount)
        {
            var dc = TestUtils.CreateContext(text);
            var activity = dc.Context.Activity;
            var result = await recognizer.RecognizeAsync(dc, activity, CancellationToken.None);

            if (ValidateIntent.ContainsKey(text))
            {
                ValidateIntent[text](result);
            }

            ValidateTelemetry(recognizer, telemetryClient, dc, activity, result, callCount);
        }
        
        internal static async Task RecognizeIntentAndValidateTelemetry_WithCustomActivity(string text, AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, int callCount)
        {
            var dc = TestUtils.CreateContext(string.Empty);
            var customActivity = Activity.CreateMessageActivity();
            customActivity.Text = text;
            customActivity.Locale = Culture.English;
            
            var result = await recognizer.RecognizeAsync(dc, (Activity)customActivity, CancellationToken.None);

            if (ValidateIntent.ContainsKey(text))
            {
                ValidateIntent[text](result);
            }

            ValidateTelemetry(recognizer, telemetryClient, dc, (Activity)customActivity, result, callCount);
        }

        internal static void ValidateTelemetry(AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, RecognizerResult result, int callCount)
        {
            var eventName = GetEventName(recognizer.GetType().Name);
            var (logPersonalInfo, error) = recognizer.LogPersonalInformation.TryGetValue(dc.State);
            var actualTelemetryProps = (IDictionary<string, string>)telemetryClient.Invocations[callCount - 1].Arguments[1];
            var expectedTelemetryProps = GetExpectedProps(activity, result, logPersonalInfo);

            telemetryClient.Verify(
                client => client.TrackEvent(
                    eventName,
                    It.Is<Dictionary<string, string>>(d => HasValidTelemetryProps(expectedTelemetryProps, actualTelemetryProps, activity)),
                    null),
                Times.Exactly(callCount));
        }

        private static string GetEventName(string recognizerName) => $"{recognizerName}Result";
        
        private static bool HasValidTelemetryProps(IDictionary<string, string> expected, IDictionary<string, string> actual, IActivity activity)
        {
            if (expected.Count == actual.Count)
            {
                foreach (var property in actual)
                {
                    if (expected.ContainsKey(property.Key))
                    {
                        if (property.Key == "Entities")
                        {
                            if (!HasValidEntities(activity, property))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (property.Value != expected[property.Key])
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private static Dictionary<string, string> GetExpectedProps(IActivity activity, RecognizerResult result, bool logPersonalInformation)
        {
            var text = activity.AsMessageActivity().Text;
            var expectedProps = ExpectedProperties.ContainsKey(text) ? ExpectedProperties[text]() : new Dictionary<string, string>();

            if (logPersonalInformation)
            {
                expectedProps.Add("Text", activity.AsMessageActivity().Text);
                expectedProps.Add("AlteredText", result.AlteredText);
            }

            return expectedProps;
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
    }
}
