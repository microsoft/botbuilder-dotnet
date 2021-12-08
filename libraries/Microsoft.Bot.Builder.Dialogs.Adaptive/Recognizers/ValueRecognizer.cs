﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.NumberWithUnit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// ValueRecognizer - Recognizer for mapping message activity. Value payload into intent/entities.
    /// </summary>
    /// <remarks>
    /// This recognizer will map MessageActivity Value payloads into intents and entities.
    ///     activity.Value.intent => RecognizerResult.Intents.
    ///     activity.Value.properties => RecognizerResult.Entities.
    /// </remarks>
    internal class ValueRecognizer : AdaptiveRecognizer
    {
        [JsonConstructor]
        internal ValueRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        public override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var recognized = new RecognizerResult { Text = activity.Text };

            if (activity.Type == ActivityTypes.Message)
            {
                // Check for submission of an adaptive card
                if (string.IsNullOrEmpty(activity.Text) && activity.Value != null)
                {
                    var value = JObject.FromObject(activity.Value);

                    // Map submitted values to a recognizer result, value : { "foo": 13} => Entities { "foo": [ 13 ] } 
                    foreach (var property in value.Properties())
                    {
                        if (property.Name.ToLowerInvariant() == "intent")
                        {
                            recognized.Intents[property.Value.ToString()] = new IntentScore { Score = 1.0 };
                        }
                        else
                        {
                            if (recognized.Entities.Property(property.Name, StringComparison.Ordinal) == null)
                            {
                                recognized.Entities[property.Name] = new JArray(property.Value);
                            }
                            else
                            {
                                ((JArray)recognized.Entities[property.Name]).Add(property.Value);
                            }
                        }
                    }
                }
            }

            TrackRecognizerResult(dialogContext, TelemetryLoggerConstants.ValueRecognizerResultEvent, FillRecognizerResultTelemetryProperties(recognized, telemetryProperties, dialogContext), telemetryMetrics);

            return Task.FromResult(recognized);
        }
    }
}
