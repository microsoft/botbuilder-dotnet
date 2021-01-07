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

            ValidateTelemetry(recognizer, telemetryClient, dc, activity, callCount);
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

            ValidateTelemetry(recognizer, telemetryClient, dc, (Activity)customActivity, callCount);
        }

        internal static void ValidateTelemetry(AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, int callCount)
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
                        if (DoesExpectedEqualActual(entry, telemetryProperties, logPersonalInformation, activity, expectedProps) == false)
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
            var text = activity.AsMessageActivity().Text;
            var expectedProps = ExpectedProperties.ContainsKey(text) ? ExpectedProperties[text]() : new Dictionary<string, string>();

            if (logPersonalInformation == true)
            {
                expectedProps.Add("Text", activity.AsMessageActivity().Text);
            }

            return expectedProps;
        }

        private static bool DoesExpectedEqualActual(KeyValuePair<string, string> entry, IDictionary<string, string> telemetryProperties, bool logPersonalInformation, IActivity activity, IDictionary<string, string> expectedProps)
        {
            var areEqual = true;
            if (IsPiiProperty(entry.Key))
            {
                if (logPersonalInformation == false)
                {
                    areEqual = false;
                }

                if (!HasCorrectPiiValue(telemetryProperties, activity))
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

        private static bool HasCorrectPiiValue(IDictionary<string, string> telemetryProperties, IActivity activity)
        {
            var userText = activity.AsMessageActivity().Text;

            if (telemetryProperties.ContainsKey("Text"))
            {
                return telemetryProperties["Text"] == userText;
            }

            return false;
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
