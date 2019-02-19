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

        public IActivityTemplate Activity { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            var activity = await Activity.BindToActivity(dialogContext.Context, dialogContext.UserState).ConfigureAwait(false); 
            await dialogContext.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
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
            this.Activity = new ActivityTemplate(text);
        }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; }

        public IActivityTemplate Activity { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            var activity = await this.Activity.BindToActivity(dialogContext.Context, dialogContext.DialogState);
            await dialogContext.Context.SendActivityAsync(activity, cancellationToken);
            return null;
        }
    }
}
