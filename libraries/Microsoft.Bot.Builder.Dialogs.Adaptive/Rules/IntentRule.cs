// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Rules
{
    /// <summary>
    /// Rule triggered when a message is received and the recognized intents & entities match a
    /// specified list of intent & entity filters.
    /// </summary>
    public class IntentRule : EventRule
    {
        [JsonConstructor]
        public IntentRule(string intent = null, List<string> entities = null, List<IDialog> steps = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(events: new List<string>()
            {
                AdaptiveEvents.RecognizedIntent
            },
            steps: steps,
            constraint: constraint,
            callerPath: callerPath, callerLine: callerLine)
        {
            Intent = intent ?? null;
            Entities = entities ?? new List<string>();
        }


        /// <summary>
        /// Intent to match on
        /// </summary>
        [JsonProperty("intent")]
        public string Intent { get; set; }

        /// <summary>
        /// Entities which must be recognized for this rule to trigger
        /// </summary>
        [JsonProperty("entities")]
        public List<string> Entities { get; set; }

        protected override Expression BuildExpression(IExpressionParser factory)
        {

            // add constraints for the intents property
            if (String.IsNullOrEmpty(this.Intent))
            {
                throw new ArgumentNullException(nameof(this.Intent));
            }


            return Expression.AndExpression(factory.Parse($"turn.dialogEvent.value.intents.{this.Intent}.score > 0.0"),
                base.BuildExpression(factory));
        }

        protected override StepChangeList OnCreateChangeList(SequenceContext planning, object dialogOptions = null)
        {
            if (planning.State.TryGetValue<RecognizerResult>("turn.dialogEvent.value", out var recognizerResult))
            {
                var (name, score) = recognizerResult.GetTopScoringIntent();
                return new StepChangeList()
                {
                    //ChangeType = this.ChangeType,

                    // proposed turn state changes

                    Turn = new Dictionary<string, object>()
                    {
                        { "recognized" , JObject.FromObject(new
                            {
                                text = recognizerResult.Text,
                                alteredText = recognizerResult.AlteredText,
                                intent = name,
                                score,
                                intents = recognizerResult.Intents,
                                entities = recognizerResult.Entities,
                            })
                        }
                    },
                    Steps = Steps.Select(s => new StepState()
                    {
                        DialogStack = new List<DialogInstance>(),
                        DialogId = s.Id,
                        Options = dialogOptions
                    }).ToList()
                };
            }

            return new StepChangeList()
            {
                Steps = Steps.Select(s => new StepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = dialogOptions
                }).ToList()
            };
        }

        public override string GetIdentity()
        {
            return $"IntentRule({this.Intent})[{String.Join(",", this.Entities)}]";
        }
    }
}
