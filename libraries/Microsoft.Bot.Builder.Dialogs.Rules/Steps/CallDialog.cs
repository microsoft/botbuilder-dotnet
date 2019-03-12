using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class CallDialog : Dialog, IDialogDependencies
    {
        public object Options { get; set; }

        public IDialog Dialog { get; set; }

        public string Property
        {
            get
            {
                return InputBindings["value"];
            }
            set
            {
                InputBindings["value"] = value;
                OutputBinding = value;
            }
        }

        public CallDialog(string id = null, string property = null, object options = null) 
            : base()
        {
            this.OutputBinding = "dialog.lastResult";
;           
            if (options != null)
            {
                this.Options = options;
            }

            if (!string.IsNullOrEmpty(property))
            {
                Property = property;
            }

            Id = id;
        }

        protected override string OnComputeId()
        {
            return $"CallDialog[{Dialog.Id}:{this.BindingPath()}]";
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Options = Options.Merge(options ?? new object());
            return await dc.BeginDialogAsync(Dialog?.Id ?? throw new Exception("CallDialog requires a dialog to be called."), Options, cancellationToken).ConfigureAwait(false);
        }

        public List<IDialog> ListDependencies()
        {
            return new List<IDialog>() { Dialog };
        }
    }
}
