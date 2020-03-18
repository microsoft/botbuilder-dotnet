// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Triggered to assign an entity to a property.
    /// </summary>
    public class OnAssignEntity : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Microsoft.OnAssignEntity";
        
        [JsonConstructor]
        public OnAssignEntity(string property = null, string entity = null, string operation = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.AssignEntity,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            Property = property;
            Entity = entity;
        }

        /// <summary>
        /// Gets or sets the property to be assigned for filtering events.
        /// </summary>
        /// <value>Property name.</value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the entity name being assigned for filtering events.
        /// </summary>
        /// <value>Entity name.</value>
        [JsonProperty("entity")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the operation being used to assign the entity for filtering events.
        /// </summary>
        /// <value>Operation name.</value>
        [JsonProperty("operation")]
        public string Operation { get; set; }

        public override string GetIdentity()
            => $"{this.GetType().Name}({this.Property}, {this.Entity})";

        public override Expression GetExpression()
        {
            var expressions = new List<Expression> { base.GetExpression() };
            if (this.Property != null)
            {
                expressions.Add(Expression.Parse($"{TurnPath.DialogEvent}.value.property == '{this.Property}'"));
            }

            if (this.Entity != null)
            {
                expressions.Add(Expression.Parse($"{TurnPath.DialogEvent}.value.entity.name == '{this.Entity}'"));
            }

            if (this.Operation != null)
            {
                expressions.Add(Expression.Parse($"{TurnPath.DialogEvent}.value.entity.operation == '{this.Operation}'"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
