// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    ///  Actions triggered when a dialog event is emitted.
    /// </summary>
    public class OnDialogEvent : OnCondition
    {
        [JsonConstructor]
        public OnDialogEvent(string @event = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(condition: condition, actions: actions, callerPath: callerPath, callerLine: callerLine)
        {
            this.Event = @event;
            this.Actions = actions ?? new List<Dialog>();
        }

        /// <summary>
        /// Gets or sets the event to fire on
        /// </summary>
        public string Event { get; set; }

        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({this.Event})";
        }

        public override Expression GetExpression(IExpressionParser factory)
        {
            return Expression.AndExpression(factory.Parse($"{TurnPath.DIALOGEVENT}.name == '{this.Event}'"), base.GetExpression(factory));
        }
    }
}
