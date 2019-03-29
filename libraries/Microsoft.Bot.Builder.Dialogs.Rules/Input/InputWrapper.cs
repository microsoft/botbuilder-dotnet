using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Input
{
    public class InputWrapper<TPrompt, TValue> : DialogCommand, IDialogDependencies where TPrompt : IDialog, new()
    {
        private TPrompt prompt;

        /// <summary>
        /// Activity to send to the user
        /// </summary>
        public ITemplate<Activity> Prompt { get; set; }

        public ITemplate<Activity> RetryPrompt { get; set; }

        public ITemplate<Activity> InvalidPrompt { get; set; }

        /// <summary>
        /// The property from memory to pass to the calling dialog and to set the return value to.
        /// </summary>
        public override string Property
        {
            get
            {
                return OutputBinding;
            }
            set
            {
                InputBindings["value"] = value;
                OutputBinding = value;
            }
        }

        public bool AlwaysPrompt { get; set; } = false;

        public InputWrapper() : base()
        {
            prompt = CreatePrompt();
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check value in state and only call if missing or required by AlwaysPrompt
            var value = dc.State.GetValue<TValue>(Property);

            if (value == null || AlwaysPrompt)
            {
                if (Prompt == null)
                {
                    throw new ArgumentNullException(nameof(Activity));
                }

                var prompt = await Prompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                var retryPrompt = RetryPrompt == null ? prompt : await RetryPrompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);

                return await dc.PromptAsync(this.prompt.Id, new PromptOptions() { Prompt = prompt, RetryPrompt = retryPrompt }, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await dc.EndDialogAsync(cancellationToken);
            }
        }

        public override List<IDialog> ListDependencies()
        {
            // Update inner prompt id before returning
            prompt.Id = Id + ":prompt";
            return new List<IDialog>() { prompt };
        }

        protected virtual TPrompt CreatePrompt()
        {
            return new TPrompt();
        }
    }
}
