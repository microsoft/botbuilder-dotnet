// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Events
{
    /// <summary>
    /// Rule triggered when a message is received and the recognized intents and entities match a
    /// specified list of intent and entity filters.
    /// </summary>
    public class OnIntent : OnDialogEvent
    {
        [JsonConstructor]
        public OnIntent(string intent = null, List<string> entities = null, List<IDialog> actions = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                events: new List<string>() { AdaptiveEvents.RecognizedIntent },
                actions: actions,
                constraint: constraint,
                callerPath: callerPath, 
                callerLine: callerLine)
            {
            Intent = intent ?? null;
            Entities = entities ?? new List<string>();
        }

        /// <summary>
        /// Gets or sets intent to match on.
        /// </summary>
        /// <value>
        /// Intent to match on.
        /// </value>
        [JsonProperty("intent")]
        public string Intent { get; set; }

        /// <summary>
        /// Gets or sets entities which must be recognized for this rule to trigger.
        /// </summary>
        /// <value>
        /// Entities which must be recognized for this rule to trigger.
        /// </value>
        [JsonProperty("entities")]
        public List<string> Entities { get; set; }

        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({this.Intent})[{string.Join(",", this.Entities)}]";
        }

        protected override Expression BuildExpression(IExpressionParser factory)
        {
            // add constraints for the intents property
            if (string.IsNullOrEmpty(this.Intent))
            {
                throw new ArgumentNullException(nameof(this.Intent));
            }

            var intentExpression = factory.Parse($"turn.recognized.intent == '{this.Intent.TrimStart('#')}'");

            // build expression to be INTENT AND (@ENTITY1 != null OR @ENTITY2 != null)
            if (this.Entities.Any())
            {
                intentExpression = Expression.AndExpression(
                    intentExpression,
                    Expression.OrExpression(this.Entities.Select(entity => factory.Parse($"exists(turn.recognized.entities.{entity.TrimStart('@')})")).ToArray()));
            }

            return Expression.AndExpression(intentExpression, base.BuildExpression(factory));
        }

        protected override ActionChangeList OnCreateChangeList(SequenceContext planning, object dialogOptions = null)
        {
            if (planning.State.TryGetValue<RecognizerResult>("turn.dialogEvent.value", out var recognizerResult))
            {
                var (name, score) = recognizerResult.GetTopScoringIntent();
                return new ActionChangeList()
                {
                    // ChangeType = this.ChangeType,

                    // proposed turn state changes
                    Turn = new Dictionary<string, object>()
                    {
                        {
                            "recognized", JObject.FromObject(new
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
                    Actions = Actions.Select(s => new ActionState()
                    {
                        DialogStack = new List<DialogInstance>(),
                        DialogId = s.Id,
                        Options = dialogOptions
                    }).ToList()
                };
            }

            return new ActionChangeList()
            {
                Actions = Actions.Select(s => new ActionState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = dialogOptions
                }).ToList()
            };
        }
    }
}
