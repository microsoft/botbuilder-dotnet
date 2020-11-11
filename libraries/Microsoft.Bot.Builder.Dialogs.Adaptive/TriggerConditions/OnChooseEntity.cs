// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Triggered to choose between different possible entity resolutions.
    /// </summary>
    public class OnChooseEntity : OnDialogEvent
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnChooseEntity";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnChooseEntity"/> class.
        /// </summary>
        /// <param name="property">Optional, property to be assigned for filtering events.</param>
        /// <param name="entity">Optional, entity name being assigned for filtering events.</param>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnChooseEntity(string property = null, string entity = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.ChooseEntity,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            Property = property;
            Entity = entity;
        }

        /// <summary>
        /// Gets or sets the property entity resolution will be assigned to for filtering events.
        /// </summary>
        /// <value>Property name.</value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the entity name that is ambiguous for filtering events.
        /// </summary>
        /// <value>Entity name.</value>
        [JsonProperty("entity")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets the identity for this rule's action.
        /// </summary>
        /// <returns>String with the identity.</returns>
        public override string GetIdentity()
            => $"{this.GetType().Name}({this.Property}, {this.Entity})";

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            var expressions = new List<Expression> { base.CreateExpression() };
            if (this.Property != null)
            {
                expressions.Add(Expression.Parse($"{TurnPath.DialogEvent}.value.property == '{this.Property}'"));
            }

            if (this.Entity != null)
            {
                expressions.Add(Expression.Parse($"{TurnPath.DialogEvent}.value.entity.name == '{this.Entity}'"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
