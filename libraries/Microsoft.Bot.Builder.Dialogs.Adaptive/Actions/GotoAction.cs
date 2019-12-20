// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Goto an action by Id.
    /// </summary>
    public class GotoAction : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.GotoAction";

        private Expression disabled;

        public GotoAction(string actionId = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.ActionId = actionId;
        }

        /// <summary>
        /// Gets or sets the action Id to goto.
        /// </summary>
        /// <value>The action Id.</value>
        public string ActionId { get; set; }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public string Disabled
        {
            get { return disabled?.ToString(); }
            set { disabled = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.disabled != null && (bool?)this.disabled.TryEvaluate(dc.GetState()).value == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Continue is simply returning an ActionScopeResult with GotoAction and actionId command in it.
            var actionScopeResult = new ActionScopeResult()
            {
                ActionScopeCommand = ActionScopeCommands.GotoAction,
                ActionId = this.ActionId ?? throw new ArgumentNullException(nameof(ActionId))
            };

            return await dc.EndDialogAsync(result: actionScopeResult, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
