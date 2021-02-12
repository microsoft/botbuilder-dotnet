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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnActivity"/> class.
        /// </summary>
        /// <param name="type">Optional, ActivityType which must be matched for this event to trigger.</param>
        /// <param name="actions">Optional, list of <see cref="Dialog"/> actions.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
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

        /// <summary>
        /// Gets the identity for the activity.
        /// </summary>
        /// <returns>Identity.</returns>
        public override string GetIdentity()
        {
            if (this.GetType() == typeof(OnActivity))
            {
                return $"{GetType().Name}({Type})[{Condition}]";
            }

            return $"{GetType().Name}[{Condition}]";
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            // add constraints for activity type
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.type == '{this.Type}'"), base.CreateExpression());
        }
    }
}
