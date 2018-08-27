using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ActivitySuggestedActionsInspector : IActivityComponentInspector
    {
        public IList<string> Inspect(Activity activity)
        {
            if(activity.SuggestedActions.Actions == null || activity.SuggestedActions.Actions.Count == 0)
            {
                return new List<string>();
            }
            else
            {
                var referencedPatterns = new List<string>();
                foreach(var action in activity.SuggestedActions.Actions)
                {
                    var actionTextReferencedPatterns = PatternRecognizer.Recognize(action.Text);
                    referencedPatterns.AddRange(actionTextReferencedPatterns);
                    var actionDisplayTextReferencedPatterns = PatternRecognizer.Recognize(action.DisplayText);
                    referencedPatterns.AddRange(actionDisplayTextReferencedPatterns);
                }
                return referencedPatterns;
            }
        }
    }
}
