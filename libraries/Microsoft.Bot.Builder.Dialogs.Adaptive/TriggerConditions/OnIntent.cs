// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when an Activity has been received and the recognized intents and entities match specified list of intent and entity filters.
    /// </summary>
    public class OnIntent : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Microsoft.OnIntent";

        [JsonConstructor]
        public OnIntent(string intent = null, List<string> entities = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.RecognizedIntent,
                actions: actions,
                condition: condition,
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

        public override Expression GetExpression()
        {
            // add constraints for the intents property
            if (string.IsNullOrEmpty(this.Intent))
            {
                throw new ArgumentNullException(nameof(this.Intent));
            }

            var intentExpression = Expression.Parse($"{TurnPath.Recognized}.intent == '{this.Intent.TrimStart('#')}'");

            // build expression to be INTENT AND (@ENTITY1 != null AND @ENTITY2 != null)
            if (this.Entities.Any())
            {
                intentExpression = Expression.AndExpression(
                    intentExpression,
                    Expression.AndExpression(this.Entities.Select(entity =>
                    {
                        if (entity.StartsWith("@") || entity.StartsWith(TurnPath.Recognized, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return Expression.Parse($"exists({entity})");
                        }

                        return Expression.Parse($"exists(@{entity})");
                    }).ToArray()));
            }

            return Expression.AndExpression(intentExpression, base.GetExpression());
        }

        protected override ActionChangeList OnCreateChangeList(ActionContext actionContext, object dialogOptions = null)
        {
            var recognizerResult = actionContext.State.GetValue<RecognizerResult>($"{TurnPath.DialogEvent}.value");
            if (recognizerResult != null)
            {
                var (name, score) = recognizerResult.GetTopScoringIntent();
                return new ActionChangeList()
                {
                    // ChangeType = this.ChangeType,

                    // proposed turn state changes
                    Turn = new Dictionary<string, object>()
                    {
                    },
                    Actions = new List<ActionState>()
                    {
                        new ActionState()
                        {
                            DialogId = this.ActionScope.Id,
                            Options = dialogOptions
                        }
                    }
                };
            }

            return base.OnCreateChangeList(actionContext, dialogOptions);
        }
    }
}
