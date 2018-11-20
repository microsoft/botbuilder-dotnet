using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.ComposableDialogs.Dialogs
{
    /// <summary>
    /// Send an activity as an action
    /// </summary>
    public class SendActivityAction : IAction
    {
        public SendActivityAction() { }

        public string ActivityId { get; set; }
        
        // public Activity Activity { get; set; }

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            Activity activity = new Activity(); //  var activity = this.Activity.GenerateActivity(dialogContext.Context, )
            await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
            return result;
        }
    }
}
