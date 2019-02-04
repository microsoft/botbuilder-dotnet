using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// CommandDialog - Call a IDialog and then execute Command when completed
    /// </summary>
    public class SequenceDialog : ComponentDialog, IDialog
    {
        public SequenceDialog(string dialogId = null) : base(dialogId)
        {
        }

        /// <summary>
        /// Sequence of steps to use for the dialogs logic
        /// </summary>
        public Sequence Sequence { get; set; }

        /// <summary>
        /// Define the expression which is the result of this dialog when it ends
        /// </summary>
        public IExpressionEval Result { get; set; }

        protected override Task OnInitialize(DialogContext outerDc)
        {
            this.InitialDialogId = this.Id + ".stepDialog";

            // if the StepDialog hasn't been added yet, add it to the dialogset
            if (this.FindDialog(InitialDialogId) == null)
            {
                if (this.Sequence.Id == null)
                {
                    this.Sequence.Id = $"Sequence.{this.Id}";
                }

                var innerDialog = new StepDialog(InitialDialogId)
                {
                    Sequence = this.Sequence,
                    Result = this.Result
                };
                this.AddDialog(innerDialog);
            }
            // make sure each step has a unique id
            for (int i = 0; i < this.Sequence.Count; i++)
            {
                var step = this.Sequence[i];
                if (String.IsNullOrEmpty(step.Id))
                {
                    step.Id = $"{this.Id}.{i}";
                }

                // add dialogs to my dialogset
                if (step is IDialogStep dialogStep)
                {
                    if (String.IsNullOrEmpty(dialogStep.Dialog.Id))
                    {
                        dialogStep.Dialog.Id = $"{this.Id}.d{i}";
                    }

                    if (this.FindDialog(dialogStep.Dialog.Id) == null && outerDc.FindDialog(dialogStep.Dialog.Id) == null)
                    {
                        this.AddDialog(dialogStep.Dialog);
                    }
                }
            }

            return base.OnInitialize(outerDc);
        }

        internal class StepDialog : Dialog, IDialog
        {
            internal StepDialog(string dialogId) : base(dialogId)
            { }

            public Sequence Sequence { get; set; }

            public IExpressionEval Result { get; set; }

            /// <summary>
            /// When this dialog is started we start the inner dialog
            /// </summary>
            /// <param name="dialogContext"></param>
            /// <param name="options"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                var state = dialogContext.ActiveDialogState;
                if (this.Sequence == null)
                {
                    return await EndThisDialog(dialogContext, null, state, cancellationToken);
                }

                state[$"{this.Id}.options"] = options;
                state[$"{this.Id}.CurrentCommandId"] = null;
                state[$"{this.Id}.Result"] = new JObject();
                return await OnTurnAsync(dialogContext, DialogReason.BeginCalled, null, cancellationToken);
            }

            /// <summary>
            /// The only time we continue is when a ContinueDialogAction is executed, in which case we start the inner dialog again (since it ended already)
            /// </summary>
            /// <param name="dialogContext"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dialogContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                return await OnTurnAsync(dialogContext, DialogReason.ContinueCalled, null, cancellationToken);
            }

            /// <summary>
            /// Inner dialog has completed
            /// </summary>
            /// <param name="dialogContext"></param>
            /// <param name="reason"></param>
            /// <param name="result"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dialogContext, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                var state = dialogContext.ActiveDialogState;
                state["DialogTurnResult"] = result;
                return await OnTurnAsync(dialogContext, DialogReason.NextCalled, result, cancellationToken);
            }

            private async Task<DialogTurnResult> OnTurnAsync(DialogContext dialogContext, DialogReason reason, object result, CancellationToken cancellationToken)
            {
                var state = dialogContext.ActiveDialogState;
                var stepResult = await this.Sequence.Execute(dialogContext, cancellationToken);
                if (stepResult is DialogTurnResult dialogTurnResult)
                {
                    switch (dialogTurnResult.Status)
                    {
                        case DialogTurnStatus.Waiting:
                        case DialogTurnStatus.Cancelled:
                            return dialogTurnResult;
                        case DialogTurnStatus.Empty:
                        case DialogTurnStatus.Complete:
                        default:
                            return await EndThisDialog(dialogContext, result, state, cancellationToken);
                    }
                }
                else
                {
                    // hit end of command, treat this as a end of dialog
                    return await EndThisDialog(dialogContext, result, state, cancellationToken);
                }
            }

            private async Task<DialogTurnResult> EndThisDialog(DialogContext dialogContext, object result, System.Collections.Generic.IDictionary<string, object> state, CancellationToken cancellationToken)
            {
                object dialogResult = result;
                if (this.Result != null)
                {
                    dialogResult = await this.Result.Evaluate(state);
                }
                return await dialogContext.EndDialogAsync(dialogResult, cancellationToken).ConfigureAwait(false);
            }

        }
    }
}
