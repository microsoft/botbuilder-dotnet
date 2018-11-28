using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Send an activity as an action
    /// </summary>
    public class SendActivity : IDialogCommand
    {
        public SendActivity() { }

        public SendActivity(string text) { this.Text = text; }

        public string Text { get; set; }

        // public Activity Activity { get; set; }

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            if (this.Text.StartsWith("{") && this.Text.EndsWith("}"))
            {
                var var = this.Text.Trim('{', '}');
                var state = dialogContext.ActiveDialog.State;
                if (state.TryGetValue(var, out object val))
                {
                    Activity activity = dialogContext.Context.Activity.CreateReply(Convert.ToString(state[var]));
                    await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
                }
                else
                {
                    await dialogContext.Context.SendActivityAsync(dialogContext.Context.Activity.CreateReply("null"), cancellationToken);
                }
            }
            else
            {
                Activity activity = dialogContext.Context.Activity.CreateReply(this.Text);
                await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
            }
            return result;
        }
    }
}
