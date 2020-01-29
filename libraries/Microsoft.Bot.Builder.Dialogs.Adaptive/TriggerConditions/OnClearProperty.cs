// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Triggered to clear a property.
    /// </summary>
    public class OnClearProperty : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Microsoft.OnClearProperty";
        
        [JsonConstructor]
        public OnClearProperty(string property = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.ClearProperty,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
            this.Property = property;
        }

        /// <summary>
        /// Gets or sets the property being cleared to filter events.
        /// </summary>
        /// <value>Property name.</value>
        [JsonProperty("property")]
        public string Property { get; set; }

        public override string GetIdentity()
            => $"{this.GetType().Name}({this.Property})";

        public override Expression GetExpression(IExpressionParser factory)
        {
            var expressions = new List<Expression> { base.GetExpression(factory) };
            if (this.Property != null)
            {
                expressions.Add(factory.Parse($"{TurnPath.DIALOGEVENT}.value.property == '{this.Property}'"));
            }

            return Expression.AndExpression(expressions.ToArray());
        }
    }
}
