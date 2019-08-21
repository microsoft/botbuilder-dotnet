// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Conditional branch.
    /// </summary>
    public class IfCondition : DialogAction, IDialogDependencies
    {
        private Expression condition;

        [JsonConstructor]
        public IfCondition([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets condition expression against memory. Example: "user.age > 18".
        /// </summary>
        /// <value>
        /// Condition expression against memory.
        /// </value>
        [JsonProperty("condition")]
        public string Condition
        {
            get
            {
                return condition?.ToString();
            }

            set
            {
                lock (this)
                {
                    condition = value != null ? new ExpressionEngine().Parse(value) : null;
                }
            }
        }

        [JsonProperty("actions")]
        public List<IDialog> Actions { get; set; } = new List<IDialog>();

        [JsonProperty("elseActions")]
        public List<IDialog> ElseActions { get; set; } = new List<IDialog>();

        public override List<IDialog> ListDependencies()
        {
            var combined = new List<IDialog>(Actions);
            combined.AddRange(ElseActions);
            return combined;
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // Ensure planning context
            if (dc is SequenceContext planning)
            {
                var (value, error) = condition.TryEvaluate(dc.State);
                var conditionResult = error == null && value != null && (bool)value;

                var actions = new List<IDialog>();
                if (conditionResult == true)
                {
                    actions = this.Actions;
                }
                else
                {
                    actions = this.ElseActions;
                }

                var planActions = actions.Select(s => new ActionState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = options
                });

                // Queue up actions that should run after current step
                planning.QueueChanges(new ActionChangeList()
                {
                    ChangeType = ActionChangeType.InsertActions,
                    Actions = planActions.ToList()
                });

                return await planning.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`IfCondition` should only be used in the context of an adaptive dialog.");
            }
        }

        protected override string OnComputeId()
        {
            var idList = Actions.Select(s => s.Id);
            return $"{nameof(IfCondition)}({this.Condition}|{string.Join(",", idList)})";
        }
    }
}
