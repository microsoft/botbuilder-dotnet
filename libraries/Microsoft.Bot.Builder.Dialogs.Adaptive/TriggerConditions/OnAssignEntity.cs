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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnAssignEntity";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnAssignEntity"/> class.
        /// </summary>
        /// <param name="property">Optional, property filter on event.</param>
        /// <param name="val">Optional, value filter on event.</param>
        /// <param name="operation">Optional, operation filter on event.</param>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnAssignEntity(string property = null, string val = null, string operation = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.AssignEntity,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            Property = property;
            Value = val;
            Operation = operation;
        }

        /// <summary>
        /// Gets or sets the property filter on events.
        /// </summary>
        /// <value>Property.</value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the value filter on events.
        /// </summary>
        /// <value>Value.</value>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the operation filter on events.
        /// </summary>
        /// <value>Operation.</value>
        [JsonProperty("operation")]
        public string Operation { get; set; }

        /// <summary>
        /// Gets the identity for this rule's action.
        /// </summary>
        /// <returns>String with the identity.</returns>
        public override string GetIdentity()
            => $"{GetType().Name}({this.Property}, {this.Value})";

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            var expressions = new List<Expression> { base.CreateExpression() };
            if (this.Property != null)
            {
                expressions.Add(Expression.Parse($"{TurnPath.DialogEvent}.value.property == '{this.Property}'"));
            }

            if (this.Value != null)
            {
                expressions.Add(Expression.Parse($"{TurnPath.DialogEvent}.value.value.name == '{this.Value}'"));
            }

            if (this.Operation != null)
            {
                expressions.Add(Expression.Parse($"{TurnPath.DialogEvent}.value.operation == '{this.Operation}'"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
