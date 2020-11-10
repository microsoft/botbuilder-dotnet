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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnIntent";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnIntent"/> class.
        /// </summary>
        /// <param name="intent">Optional, intent to match on.</param>
        /// <param name="entities">Optional, entities which must be recognized for this rule to trigger.</param>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
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
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<string> Entities { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets the identity for this rule's action.
        /// </summary>
        /// <returns>String with the identity.</returns>
        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({this.Intent})[{string.Join(",", this.Entities)}]";
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            // add constraints for the intents property
            if (string.IsNullOrEmpty(this.Intent))
            {
                throw new InvalidOperationException($"The {nameof(this.Intent)} property is null or empty.");
            }

            var intentExpression = Expression.Parse($"{TurnPath.Recognized}.intent == '{this.Intent.TrimStart('#')}'");

            // build expression to be INTENT AND (@ENTITY1 != null AND @ENTITY2 != null)
            if (this.Entities.Any())
            {
                intentExpression = Expression.AndExpression(
                    intentExpression,
                    Expression.AndExpression(this.Entities.Select(entity =>
                    {
                        if (entity.StartsWith("@", StringComparison.Ordinal) || entity.StartsWith(TurnPath.Recognized, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return Expression.Parse($"exists({entity})");
                        }

                        return Expression.Parse($"exists(@{entity})");
                    }).ToArray()));
            }

            return Expression.AndExpression(intentExpression, base.CreateExpression());
        }

        /// <summary>
        /// Called when a change list is created.
        /// </summary>
        /// <param name="actionContext">Context to use for evaluation.</param>
        /// <param name="dialogOptions">Optional, object with dialog options.</param>
        /// <returns>An <see cref="ActionChangeList"/> with the list of actions.</returns>
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
