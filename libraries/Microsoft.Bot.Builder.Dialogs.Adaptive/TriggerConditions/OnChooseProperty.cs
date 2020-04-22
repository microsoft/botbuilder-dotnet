// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Triggered to choose which property an entity goes to.
    /// </summary>
    public class OnChooseProperty : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnChooseProperty";
        
        [JsonConstructor]
        public OnChooseProperty(List<string> properties = null, List<string> entities = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.ChooseProperty,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            this.Properties = properties ?? new List<string>();
            this.Entities = entities ?? new List<string>();
        }

        /// <summary>
        /// Gets or sets the properties being chosen between to filter events.
        /// </summary>
        /// <value>List of property names.</value>
        [JsonProperty("properties")]
        public List<string> Properties { get; set; }

        /// <summary>
        /// Gets or sets the entities being chosen between to filter events.
        /// </summary>
        /// <value>List of entity names.</value>
        [JsonProperty("entities")]
        public List<string> Entities { get; set; }

        public override string GetIdentity()
            => $"{this.GetType().Name}([{string.Join(",", this.Properties)}], {this.Entities})";

        public override Expression GetExpression()
        {
            var expressions = new List<Expression> { base.GetExpression() };
            foreach (var property in this.Properties)
            {
                expressions.Add(Expression.Parse($"contains(foreach({TurnPath.DialogEvent}.value, mapping, mapping.property), '{property}')"));
            }

            foreach (var entity in this.Entities)
            {
                expressions.Add(Expression.Parse($"contains(foreach({TurnPath.DialogEvent}.value, mapping, mapping.entity.name), '{entity}')"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
