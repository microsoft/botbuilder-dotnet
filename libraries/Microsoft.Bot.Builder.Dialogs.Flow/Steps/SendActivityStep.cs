using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    public class SendActivityTemplateStep : IStep
    {
        public SendActivityTemplateStep() { }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; }

        public ActivityTemplate Activity { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            var activity = Activity.Bind(dialogContext.UserState);
            await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
            return null;
        }
    }

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
            await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
            return null;
        }
    }
}
