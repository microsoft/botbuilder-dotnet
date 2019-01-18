using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Replace the current Dialog with another dialog as an action
    /// </summary>
    public class GotoDialog : IDialogStep
    {
        public GotoDialog() { }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("n");

        /// <summary>
        /// The dialog to call
        /// </summary>
        public IDialog Dialog { get; set; }

        /// <summary>
        /// The options for calling the dialog
        /// </summary>
        public object Options { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            return await dialogContext.ReplaceDialogAsync(this.Dialog.Id, Options, cancellationToken);
        }
    }
}
