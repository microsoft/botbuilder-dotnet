// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Class which allows you to edit the current actions. 
    /// </summary>
    public class EditActions : Dialog, IDialogDependencies
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.EditActions";

        /// <summary>
        /// Initializes a new instance of the <see cref="EditActions"/> class.
        /// </summary>
        /// <param name="sourceFilePath">Source file.</param>
        /// <param name="sourceLineNumber">Line number.</param>
        [JsonConstructor]
        public EditActions([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

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
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets the actions to be applied to the active action.
        /// </summary>
        /// <value>
        /// The actions to be applied to the active action.
        /// </value>
        [JsonProperty("actions")]
        public List<Dialog> Actions { get; set; } = new List<Dialog>();

        /// <summary>
        /// Gets or sets the type of change to appy to the active actions.
        /// </summary>
        /// <value>
        /// The type of change to appy to the active actions.
        /// </value>
        [JsonProperty("changeType")]
        public EnumExpression<ActionChangeType> ChangeType { get; set; } = new EnumExpression<ActionChangeType>();

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            return this.Actions;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (dc.Parent is ActionContext ac)
            {
                var planActions = Actions.Select(s => new ActionState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = options
                });

                var changes = new ActionChangeList()
                {
                    ChangeType = ChangeType.GetValue(dc.State),
                    Actions = planActions.ToList()
                };

                ac.QueueChanges(changes);

                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`EditActions` should only be used in the context of an adaptive dialog.");
            }
        }

        protected override string OnComputeId()
        {
            var idList = Actions.Select(s => s.Id);
            return $"{this.GetType().Name}[{this.ChangeType?.ToString()}|{string.Join(",", idList)}]";
        }
    }
}
