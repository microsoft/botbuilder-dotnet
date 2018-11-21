using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.FlowDialogs
{
    /// <summary>
    /// Send an activity as an action
    /// </summary>
    public class SendActivity : IFlowCommand
    {
        public SendActivity() { }

        public SendActivity(string text) { this.Text = text; }

        public string Text { get; set; }
        
        // public Activity Activity { get; set; }

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            Activity activity = dialogContext.Context.Activity.CreateReply(this.Text);
            await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
            return result;
        }
    }
}
