using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Send an activity as an action
    /// </summary>
    public class SendActivityStep : IStep
    {
        public SendActivityStep() { }

        public SendActivityStep(string text)
        {
            this.Activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Text = text
            };
        }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; }

        public Activity Activity { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            var activity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(Activity));
            if (activity.Text.StartsWith("{") && activity.Text.EndsWith("}"))
            {
                var var = activity.Text.Trim('{', '}');
                var state = dialogContext.ActiveDialog.State;
                if (state.TryGetValue(var, out object val))
                {
                    activity.Text = Convert.ToString(state[var]);
                    await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
                }
                else
                {
                    await dialogContext.Context.SendActivityAsync(dialogContext.Context.Activity.CreateReply("null"), cancellationToken);
                }
            }
            else
            {
                await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
            }
            return null;
        }
    }
}
