using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    interface IActivityGenerator 
    {
        /// <summary>
        /// ActivityGenerator
        /// </summary>
        /// <remarks>Generate an activity for templateId, locale</remarks>
        Task<Activity> GenerateActivity(ITurnContext context, string templateId, params object[] args);
    }
}
