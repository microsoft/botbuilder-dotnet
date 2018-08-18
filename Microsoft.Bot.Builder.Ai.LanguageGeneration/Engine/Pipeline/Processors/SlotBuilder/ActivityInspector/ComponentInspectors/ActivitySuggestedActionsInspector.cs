using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ActivitySuggestedActionsInspector : IActivityComponentInspector
    {
        public async Task<IList<string>> InspectAsync(Activity activity)
        {
            throw new NotImplementedException();
        }
    }
}
