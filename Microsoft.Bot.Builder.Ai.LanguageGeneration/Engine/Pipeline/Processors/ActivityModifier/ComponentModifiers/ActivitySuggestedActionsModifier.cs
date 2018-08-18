using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ActivitySuggestedActionsModifier : IActivityComponentModifier
    {
        public Task ModifyAsync(Activity activity, ICompositeResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
