// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    /// <summary>
    /// Triggered when a form needs to choose which value to assign to a singleton.
    /// </summary>
    public class OnUnknownEntity : OnDialogEvent
    {
        [JsonConstructor]
        public OnUnknownEntity(string entity = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: FormEvents.UnknownEntity,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            this.Entity = entity;
        }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        public override string GetIdentity()
            => $"{this.GetType().Name}({this.Entity})";

        public override Expression GetExpression(IExpressionParser factory)
        {
            var expressions = new List<Expression> { base.GetExpression(factory) };
            if (this.Entity != null)
            {
                expressions.Add(factory.Parse($"{TurnPath.DIALOGEVENT}.value.entity.name == '{this.Entity}'"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
