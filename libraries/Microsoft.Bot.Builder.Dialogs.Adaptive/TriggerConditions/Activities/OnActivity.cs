// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when a Activity of a given type is received. 
    /// </summary>
    public class OnActivity : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnActivity";

        [JsonConstructor]
        public OnActivity(string type = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(@event: AdaptiveEvents.ActivityReceived, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets the ActivityType which must be matched for this to trigger.
        /// </summary>
        /// <value>
        /// ActivityType.
        /// </value>
        [JsonProperty("type")]
        public string Type { get; set; }

        public override string GetIdentity()
        {
            if (this.GetType() == typeof(OnActivity))
            {
                return $"{this.GetType().Name}({this.Type})[{this.Condition}]";
            }

            return $"{this.GetType().Name}[{this.Condition}]";
        }

        public override Expression GetExpression()
        {
            // add constraints for activity type
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.type == '{this.Type}'"), base.GetExpression());
        }
    }
}
