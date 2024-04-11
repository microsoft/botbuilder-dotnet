// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when an error event has been emitted.
    /// </summary>
    public class OnError : OnDialogEvent
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnError";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnError"/> class.
        /// </summary>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnError(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(@event: AdaptiveEvents.Error, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the number of executions allowed. Used to avoid infinite loops in case of error.
        /// </summary>
        /// <value>The number of executions allowed for this trigger.</value>
        [JsonProperty("executionLimit")]
        public NumberExpression ExecutionLimit { get; set; } = new NumberExpression();

        /// <summary>
        /// Method called to execute the rule's actions.
        /// </summary>
        /// <param name="actionContext">Context.</param>
        /// <returns>A <see cref="Task"/> with plan change list.</returns>
        public override Task<List<ActionChangeList>> ExecuteAsync(ActionContext actionContext)
        {
            // 10 is the default number of executions we'll allow before breaking the loop.
            var limit = ExecutionLimit.Value > 0 ? ExecutionLimit.Value : 10;
            actionContext.State.SetValue(TurnPath.ExecutionLimit, limit);
            return base.ExecuteAsync(actionContext);
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

            // For OnError handling we want to replace the old plan with whatever the error plan is, since the old plan blew up.
            changeList.ChangeType = ActionChangeType.ReplaceSequence;
            return changeList;
        }
    }
}
