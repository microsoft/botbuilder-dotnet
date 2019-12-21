// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Triggered to choose between different possible entity resolutions.
    /// </summary>
    public class OnChooseEntity : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Microsoft.OnChooseEntity";
        
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

        public override string GetIdentity()
            => $"{this.GetType().Name}({this.Property}, {this.Entity})";

        public override Expression GetExpression(IExpressionParser factory)
        {
            var expressions = new List<Expression> { base.GetExpression(factory) };
            if (this.Property != null)
            {
                expressions.Add(factory.Parse($"{TurnPath.DIALOGEVENT}.value.property == '{this.Property}'"));
            }

            if (this.Entity != null)
            {
                expressions.Add(factory.Parse($"{TurnPath.DIALOGEVENT}.value.entity.name == '{this.Entity}'"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
