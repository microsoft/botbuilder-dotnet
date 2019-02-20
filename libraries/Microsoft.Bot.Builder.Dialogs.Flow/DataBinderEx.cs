using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    public static class DataBinderEx
    {
        public static ITemplate<IMessageActivity> AddMessageActivityTemplate(this IStep step, string name)
        {
            return new MessagePropertyTemplate(DataBinding.GetTypes(step.GetType()), name);
        }
    }
}
