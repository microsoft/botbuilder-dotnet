using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ActivitySuggestedActionsModifier : IActivityComponentModifier
    {
        public void Modify(Activity activity, ICompositeResponse response)
        {
            if (!string.IsNullOrWhiteSpace(activity.Text))
            {
                var recognizedPatterns = PatternRecognizer.Recognize(activity.Text);
                foreach (var pattern in recognizedPatterns)
                {
                    activity.Text = activity.Text.Replace(pattern, response.TemplateResolutions[pattern]);
                }
            }


            if (!(activity.SuggestedActions.Actions == null || activity.SuggestedActions.Actions.Count == 0))
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
