using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{

    public class ActivityTemplate : IActivityTemplate
    {
        public ActivityTemplate()
        {
        }

        public ActivityTemplate(Activity activity)
        {
            this.Activity = activity;
        }

        public Activity Activity { get; set; }

        public Task<Activity> BindToActivity(ITurnContext context, object data)
        {
            return Task.FromResult(Activity);
        }
    }
}
