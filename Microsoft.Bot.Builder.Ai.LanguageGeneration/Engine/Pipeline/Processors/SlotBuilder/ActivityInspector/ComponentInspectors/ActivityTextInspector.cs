using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ActivityTextInspector : IActivityComponentInspector
    {
        public IList<string> Inspect(Activity activity)
        {
            if (string.IsNullOrWhiteSpace(activity.Text))
            {
                return new List<string>();
            }
            else
            {
                return PatternRecognizer.Recognize(activity.Text);
            }
        }
    }
}
