// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    /// <summary>
    /// Helper class used to validate telemetry properties of subclasses of <see cref="AdaptiveRecognizer"/>.
    /// </summary>
    internal class RecognizerTelemetryUtils
    {
        internal static readonly string CodeIntentText = "intent a1 b2";

        internal static readonly string ColorIntentText = "I would like color red and orange";

        internal static readonly string GreetingIntentTextEnUs = "howdy";

        internal static readonly string CrossTrainText = "criss-cross applesauce";

        internal static readonly string X = "x";

        /// <summary>
        /// Get the expected properties based on the text/utterance that we run the recognizer against.
        /// </summary>
        private static readonly Dictionary<string, Func<Dictionary<string, string>>> ExpectedProperties = new Dictionary<string, Func<Dictionary<string, string>>>
        {
            { CodeIntentText, GetCodeIntentProperties },
            { ColorIntentText, GetColorIntentProperties },
            { GreetingIntentTextEnUs, GetGreetingIntentProperties },
            { CrossTrainText, GetChooseIntentProperties },
            { X, GetXIntentProperties }
        };

        /// <summary>
        /// Run the expected validations based on intent recognized.
        /// </summary>
        private static readonly Dictionary<string, Action<RecognizerResult>> ValidateIntent = new Dictionary<string, Action<RecognizerResult>>
        {
            { CodeIntentText, ValidateCodeIntent },
            { ColorIntentText, ValidateColorIntent },
            { GreetingIntentTextEnUs, ValidateGreetingIntent },
            { CrossTrainText, ValidateChooseIntent },
            { X, ValidateXIntent }
        };

        /// <summary>
        /// Calls the recognizer's `RecognizeAsync` method and validates appropriate telemetry properties are logged.
        /// </summary>
        /// <param name="text">The activity's text used run recognition against.</param>
        /// <param name="recognizer">The recognizer used to call `RecognizeAsync`.</param>
        /// <param name="telemetryClient">The telemetry client used to log telemetry.</param>
        /// <param name="callCount">How many times the telemetry client should have logged the `RecognizerResult` of our target recognizer.</param>
        /// <returns>Task representing the validation work done.</returns>
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

        /// <summary>
        /// Calls the recognizer's `RecognizeAsync` method and validates appropriate telemetry properties are logged,
        /// using a custom activity, separate from the activity found in <see cref="DialogContext"/>.
        /// </summary>
        /// <param name="text">The activity's text used run recognition against.</param>
        /// <param name="recognizer">The recognizer used to call `RecognizeAsync`.</param>
        /// <param name="telemetryClient">The telemetry client used to log telemetry.</param>
        /// <param name="callCount">How many times the telemetry client should have logged the `RecognizerResult` of our target recognizer.</param>
        /// <returns>Task representing the validation work done.</returns>
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

        /// <summary>
        /// Ensure recognizer correctly logs telemetry.
        /// 
        /// More specifically, verify that <see cref="IBotTelemetryClient"/>.TrackEvent is called with:
        /// <list type="bullet">
        ///     <item>Appropriate event name (e.g. "RegexRecognizerResult" for <see cref="RegexRecognizer"/>.</item>
        ///     <item>Recognizer properly called <see cref="IBotTelemetryClient.TrackEvent(string, IDictionary{string, string}, IDictionary{string, double})"/> method to log telemetry with correct telemetry properties.</item>
        ///     <item><see cref="IBotTelemetryClient"/>.TrackEvent is called correct number of times.</item>
        /// </list>
        /// </summary>
        /// <param name="recognizer">The recognizer used to call `RecognizeAsync` and, in turn, that logged telemetry.</param>
        /// <param name="telemetryClient">The telemetry client used to log telemetry.</param>
        /// <param name="dc">The <see cref="DialogContext"/>.</param>
        /// <param name="activity">The activity used to recognize intent with in `RecognizeAsync`.</param>
        /// <param name="result">The <see cref="RecognizerResult"/>.</param>
        /// <param name="callCount">How many times the telemetry client should have logged the `RecognizerResult` of our target recognizer.</param>
        internal static void ValidateTelemetry(AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, RecognizerResult result, int callCount)
        {
            var eventName = $"{recognizer.GetType().Name}Result";
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

        /// <summary>
        /// Validate that the actual telemetry properties logged match the telemetry properties we expect to log.
        /// </summary>
        /// <param name="expected">The telemetry properties expected to log.</param>
        /// <param name="actual">The actual telemetry properties logged.</param>
        /// <param name="activity">The activity used in `Recognizer.RecognizerAsync` to recognize intent with.</param>
        /// <returns>A boolean value.</returns>
        private static bool HasValidTelemetryProps(IDictionary<string, string> expected, IDictionary<string, string> actual, IActivity activity)
        {
            if (expected.Count != actual.Count)
            {
                return false;
            }

            foreach (var property in actual)
            {
                if (!expected.ContainsKey(property.Key))
                {
                    return false;
                }

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

            return true;
        }

        /// <summary>
        /// Get expected properties based on the <see cref="Activity"/>.Text that was used to recognize intent with in `RecognizeAsync`.
        /// Telemetry properties logged should also differ, depending on value of <see cref="AdaptiveRecognizer.LogPersonalInformation"/>.
        /// </summary>
        /// <param name="activity">The activity used in `Recognizer.RecognizerAsync` to recognize intent with.</param>
        /// <param name="result">The <see cref="RecognizerResult"/>.</param>
        /// <param name="logPersonalInformation">Flag used to determine whether or not to log personally identifiable information.</param>
        /// <returns>Dictionary of expected telemetry properties.</returns>
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

        private static bool HasValidEntities(IActivity activity, KeyValuePair<string, string> property)
        {
            var text = activity.AsMessageActivity().Text;
            var actualEntity = JsonConvert.DeserializeObject<Dictionary<string, object>>(property.Value);

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
