// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;

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
                PlanningEvents.UtteranceRecognized.ToString()
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
        public string Intent { get; set; }

        /// <summary>
        /// Entities which must be recognized for this rule to trigger
        /// </summary>
        public List<string> Entities { get; set; }


        protected override Expression BuildExpression(IExpressionParser factory)
        {
            List<Expression> expressions = new List<Expression>();
            // add constraints for the intents property
            if (!String.IsNullOrEmpty(this.Intent))
            {
                expressions.Add(factory.Parse($"turn.DialogEvent.Value.Intents.{this.Intent}.Score > 0.0"));
            }

            //foreach (var entity in this.Entities)
            //{
            //    constraints.Add($"CONTAINS(DialogEvent.Entities, '{entity}')");
            //}
            return Expression.AndExpression(Expression.AndExpression(expressions.ToArray()), base.BuildExpression(factory));
        }

        protected override PlanChangeList OnCreateChangeList(PlanningContext planning, object dialogOptions = null)
        {
            var dialogEvent = planning.State.Turn["DialogEvent"] as DialogEvent;
            if (dialogEvent.Value is RecognizerResult recognizerResult)
            {
                Dictionary<string, object> entitiesRecognized = new Dictionary<string, object>();
                entitiesRecognized = recognizerResult.Entities.ToObject<Dictionary<string, object>>();

                return new PlanChangeList()
                {
                    //ChangeType = this.ChangeType,
                    IntentsMatched = new List<string> {
                        this.Intent,
                    },
                    EntitiesMatched = this.Entities,
                    EntitiesRecognized = entitiesRecognized,
                    Steps = Steps.Select(s => new PlanStepState()
                    {
                        DialogStack = new List<DialogInstance>(),
                        DialogId = s.Id,
                        Options = dialogOptions
                    }).ToList()
                };
            }

            return new PlanChangeList()
            {
                Steps = Steps.Select(s => new PlanStepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = dialogOptions
                }).ToList()
            };
        }
    }
}
