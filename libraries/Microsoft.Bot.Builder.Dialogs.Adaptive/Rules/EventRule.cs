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

        protected override Expression BuildExpression(IExpressionParser factory)
        {
            List<Expression> expressions = new List<Expression>();

            foreach (var evt in Events)
            {
                expressions.Add(factory.Parse($"turn.DialogEvent.Name == '{evt}'"));
            }

            return Expression.AndExpression(Expression.OrExpression(expressions.ToArray()), base.BuildExpression(factory));
        }

    }
}
