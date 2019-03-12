using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class EndDialog : DialogCommand
    {
        /// <summary>
        /// Specifies an in-memory state property that should be returned to the calling  dialog.
        /// </summary>
        public string ResultProperty { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = ResultProperty != null ? dc.State.GetValue<string>(ResultProperty) : null;
            return await EndParentDialogAsync(dc, result, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"end({this.ResultProperty ?? string.Empty})";
        }
    }
}
