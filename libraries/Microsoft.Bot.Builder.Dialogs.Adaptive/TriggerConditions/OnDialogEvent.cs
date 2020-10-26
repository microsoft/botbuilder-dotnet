// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    ///  Actions triggered when a dialog event is emitted.
    /// </summary>
    public class OnDialogEvent : OnCondition
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnDialogEvent";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnDialogEvent"/> class.
        /// </summary>
        /// <param name="event">Optional, the event to fire on.</param>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnDialogEvent(string @event = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(condition: condition, actions: actions, callerPath: callerPath, callerLine: callerLine)
        {
            this.Event = @event;
            this.Actions = actions ?? new List<Dialog>();
        }

        /// <summary>
        /// Gets or sets the event to fire on.
        /// </summary>
        /// <value>
        /// The event to fire on.
        /// </value>
        public string Event { get; set; }

        /// <summary>
        /// Gets the identity for this rule's action.
        /// </summary>
        /// <returns>String with the identity.</returns>
        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({this.Event})";
        }

        /// <summary>
        /// Gets the expression for this rule.
        /// </summary>
        /// <returns>Expression which will be cached and used to evaluate this rule.</returns>
        public override Expression GetExpression()
        {
            return Expression.AndExpression(Expression.Parse($"{TurnPath.DialogEvent}.name == '{this.Event}'"), base.GetExpression());
        }

        /// <summary>
        /// Called when a change list is created.
        /// </summary>
        /// <param name="actionContext">Context to use for evaluation.</param>
        /// <param name="dialogOptions">Optional, object with dialog options.</param>
        /// <returns>An <see cref="ActionChangeList"/> with the list of actions.</returns>
        protected override ActionChangeList OnCreateChangeList(ActionContext actionContext, object dialogOptions = null)
        {
            var changeList = base.OnCreateChangeList(actionContext, dialogOptions);
            var current = actionContext.ActiveDialog.Version;
            var version = actionContext.Dialogs.GetVersion();

            if (current == null || current == version)
            {
                changeList.ChangeType = ActionChangeType.AppendActions;
            }

            return changeList;
        }
    }
}
