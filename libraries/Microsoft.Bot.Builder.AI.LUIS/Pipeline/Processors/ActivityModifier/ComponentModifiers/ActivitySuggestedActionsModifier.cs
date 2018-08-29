using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal class ActivitySuggestedActionsModifier : IActivityComponentModifier
    {
        public void Modify(Activity activity, ICompositeResponse response)
        {
            if ((activity.SuggestedActions != null) && !(activity.SuggestedActions.Actions == null || activity.SuggestedActions.Actions.Count == 0))
            {
                var referencedPatterns = new List<string>();
                foreach (var action in activity.SuggestedActions.Actions)
                {

                    var recognizedPatterns = PatternRecognizer.Recognize(action.Text);
                    foreach (var pattern in recognizedPatterns)
                    {
                        action.Text = action.Text.Replace(pattern, response.TemplateResolutions[pattern]);
                    }

                    recognizedPatterns = PatternRecognizer.Recognize(action.DisplayText);
                    foreach (var pattern in recognizedPatterns)
                    {
                        action.DisplayText = action.DisplayText.Replace(pattern, response.TemplateResolutions[pattern]);
                    }
                }
            }
        }
    }
}
