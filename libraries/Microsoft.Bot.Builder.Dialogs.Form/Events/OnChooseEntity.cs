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
    /// Triggered when a form needs to choose which entity to assign to a singleton property.
    /// </summary>
    public class OnChooseEntity : OnDialogEvent
    {
        [JsonConstructor]
        public OnChooseEntity(string property = null, List<string> entities = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: FormEvents.ChooseEntity,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            Property = property;
            Entities = entities;
        }

        [JsonProperty("property")]
        public string Property { get; set; }

        [JsonProperty("entities")]
        public List<string> Entities { get; set; }

        public override string GetIdentity()
            => $"{this.GetType().Name}({this.Property}, [{string.Join(",", this.Entities)}])";

        public override Expression GetExpression(IExpressionParser factory)
        {
            var expressions = new List<Expression> { base.GetExpression(factory) };
            if (this.Property != null)
            {
                expressions.Add(factory.Parse($"{TurnPath.DIALOGEVENT}.value.change.property == '{this.Property}'"));
            }

            if (this.Entities != null)
            {
                foreach (var entity in this.Entities)
                {
                    expressions.Add(factory.Parse($"contains(foreach(entity, {TurnPath.DIALOGEVENT}.value.entities, entity.name), '{entity}')"));
                }
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
