// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Rules
{
    /// <summary>
    /// Rule triggered when a dialog event matching a list of event names is emitted.
    /// </summary>
    public class EventRule : Rule
    {
        /// <summary>
        /// List of events to filter
        /// </summary>
        public List<string> Events { get; set; }

        [JsonConstructor]
        public EventRule(List<string> events = null, List<IDialog> steps = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(constraint: constraint, steps: steps, callerPath: callerPath, callerLine: callerLine)
        {
            this.Events = events ?? new List<string>();
            this.Steps = steps ?? new List<IDialog>();
        }

        protected override StepChangeList OnCreateChangeList(SequenceContext planning, object dialogOptions = null)
        {
            var changeList = new StepChangeList()
            {
                ChangeType = StepChangeTypes.InsertSteps,
                Steps = new List<StepState>()
            };

            this.Steps.ForEach(s =>
            {
                var stepState = new StepState()
                {
                    DialogId = s.Id,
                    DialogStack = new List<DialogInstance>()
                };

                if (dialogOptions != null)
                {
                    stepState.Options = dialogOptions;
                }

                changeList.Steps.Add(stepState);
            });

            return changeList;
        }

        protected override Expression BuildExpression(IExpressionParser factory)
        {
            List<Expression> expressions = new List<Expression>();

            foreach (var evt in Events)
            {
                expressions.Add(factory.Parse($"turn.dialogEvent.name == '{evt}'"));
            }

            return Expression.AndExpression(Expression.OrExpression(expressions.ToArray()), 
                base.BuildExpression(factory));
        }

        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({string.Join(",", Events)})";
        }
    }
}
