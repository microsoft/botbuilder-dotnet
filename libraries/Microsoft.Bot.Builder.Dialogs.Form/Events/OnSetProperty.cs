// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    /// <summary>
    /// Triggered when a form needs to set a slot to an entity.
    /// </summary>
    public class OnSetProperty : OnDialogEvent
    {
        [JsonConstructor]
        public OnSetProperty(string property = null, string entity = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: FormEvents.SetPropertyToEntity,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            Property = property;
            Entity = entity;
        }

        [JsonProperty("property")]
        public string Property { get; set; }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        public override string GetIdentity()
            => $"{this.GetType().Name}({this.Property}, {this.Entity})";

        public override Expression GetExpression(IExpressionParser factory)
        {
            var expressions = new List<Expression> { base.GetExpression(factory) };
            if (this.Property != null)
            {
                expressions.Add(factory.Parse($"{TurnPath.DIALOGEVENT}.change.property == '{this.Property}'"));
            }

            if (this.Entity != null)
            {
                expressions.Add(factory.Parse($"{TurnPath.DIALOGEVENT}.entity.name == '{this.Entity}'"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
