using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    /// <summary>
    /// Send an activity and finish
    /// </summary>
    public class SendActivityDialog : Dialog, IDialog 
    {
        public SendActivityDialog(string id = null) : base(id) { }

        public Activity Activity { get; set; }

        /// <summary>
        /// Use recognizer intent to invoke sub dialog
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await dc.Context.SendActivityAsync(this.Activity, cancellationToken);
            return await dc.EndDialogAsync();
        }
    }
}
