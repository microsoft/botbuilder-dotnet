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
    /// Triggered when a form needs to choose between multiple different mappings.
    /// </summary>
    public class OnChooseMapping: OnDialogEvent
    {
        [JsonConstructor]
        public OnChooseMapping(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: FormEvents.ChooseMapping,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
        }


        [JsonProperty("properties")]
        public List<string> Properties { get; set; }

        [JsonProperty("entities")]
        public string Entities { get; set; }

        public override string GetIdentity()
            => $"{this.GetType().Name}([{string.Join(",", this.Properties)}], [{string.Join(",", this.Entities)}])";

        public override Expression GetExpression(IExpressionParser factory)
        {
            var expressions = new List<Expression> { base.GetExpression(factory) };
            if (this.Properties != null)
            {
                foreach (var property in this.Properties)
                {
                    expressions.Add(factory.Parse($"contains(foreach(mapping, {TurnPath.DIALOGEVENT}.mappings, mapping.property), '{property}')"));
                }
            }

            if (this.Entities != null)
            {
                foreach (var entity in this.Entities)
                {
                    expressions.Add(factory.Parse($"contains(foreach(mapping, {TurnPath.DIALOGEVENT}.mappings, mapping.entity.name), '{entity}')"));
                }
            }

            return Expression.AndExpression(expressions.ToArray());
        }

    }
}

