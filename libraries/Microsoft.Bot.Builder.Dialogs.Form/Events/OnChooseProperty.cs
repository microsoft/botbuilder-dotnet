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
    /// Triggered when a form needs choose which property an entity goes to.
    /// </summary>
    public class OnChooseProperty : OnDialogEvent
    {
        [JsonConstructor]
        public OnChooseProperty(List<string> properties = null, string entity = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: FormEvents.ChooseProperty,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            this.Properties = properties;
            this.Entity = entity;
        }

        [JsonProperty("properties")]
        public List<string> Properties { get; set; }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        public override string GetIdentity()
            => $"{this.GetType().Name}([{string.Join(",", this.Properties)}], {this.Entity})";

        public override Expression GetExpression(IExpressionParser factory)
        {
            var expressions = new List<Expression> { base.GetExpression(factory) };
            if (this.Properties != null)
            {
                foreach (var property in this.Properties)
                {
                    expressions.Add(factory.Parse($"contains(foreach(property, {TurnPath.DIALOGEVENT}.value.properties, property.name), '{property}')"));
                }
            }

            if (this.Entity != null)
            {
                expressions.Add(factory.Parse($"{TurnPath.DIALOGEVENT}.value.entity.name == '{this.Entity}'"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
