// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when a custom dialog event is emitted.
    /// </summary>
    public class OnCustomEvent : OnCondition
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnCustomEvent";

        [JsonConstructor]
        public OnCustomEvent(string @event = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(condition: condition, actions: actions, callerPath: callerPath, callerLine: callerLine)
        {
            this.Event = @event;
            this.Actions = actions ?? new List<Dialog>();
        }

        /// <summary>
        /// Gets or sets the custom event to fire on.
        /// </summary>
        /// <value>
        /// The custom event to fire on.
        /// </value>
        public string Event { get; set; }

        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({this.Event})";
        }

        public override Expression GetExpression()
        {
            return Expression.AndExpression(Expression.Parse($"{TurnPath.DialogEvent}.name == '{this.Event}'"), base.GetExpression());
        }
    }
}
