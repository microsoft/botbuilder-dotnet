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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnChooseProperty";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnChooseProperty"/> class.
        /// </summary>
        /// <param name="properties">Optional, list of properties being chosen between to filter events.</param>
        /// <param name="entities">Optional, list of entities being chosen between to filter events.</param>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
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
#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> Properties { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the entities being chosen between to filter events.
        /// </summary>
        /// <value>List of entity names.</value>
        [JsonProperty("entities")]
#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> Entities { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets the identity for this rule's action.
        /// </summary>
        /// <returns>String with the identity.</returns>
        public override string GetIdentity()
            => $"{this.GetType().Name}([{string.Join(",", this.Properties)}], {this.Entities})";

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            var expressions = new List<Expression> { base.CreateExpression() };
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
