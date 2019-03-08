using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Planning.Steps
{
    public class GotoDialog : DialogCommand
    {
        public string DialogId { get; set; }

        public object Options { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            object parentOptions;

            if (options != null && Options != null)
            {
                parentOptions = options.Merge<object>(Options);
            }
            else if (options == null)
            {
                parentOptions = Options;
            }
            else if (Options == null)
            {
                parentOptions = Options;
            }

            return await ReplaceParentDialogAsync(dc, DialogId, options, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"Goto({DialogId})";
        }
    }
}
