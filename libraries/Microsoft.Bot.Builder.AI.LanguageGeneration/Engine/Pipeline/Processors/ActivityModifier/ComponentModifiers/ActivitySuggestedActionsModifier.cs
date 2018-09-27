using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    // <summary>
    /// Class to modify/substitute a <see cref="Activity"/> object for template references in <see cref="Activity.SuggestedActions"/>. 
    /// </summary>
    internal class ActivitySuggestedActionsModifier : IActivityComponentModifier
    {
        /// <summary>
        /// Modify/substitute  a <see cref="Activity"/> object for template references in <see cref="Activity.SuggestedActions"/>. 
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be modified.</param>
        /// <param name="response">The <see cref="ICompositeResponse"/> object that carries the tempolate resolution values, which will be used to modify the activity.</param>
        public void Modify(Activity activity, ICompositeResponse response)
        {
            if ((activity.SuggestedActions != null) && !(activity.SuggestedActions.Actions == null || activity.SuggestedActions.Actions.Count == 0))
            {
                foreach (var action in activity.SuggestedActions.Actions)
                {

                    var recognizedPatterns = PatternRecognizer.Recognize(action.Text);
                    foreach (var pattern in recognizedPatterns)
                    {
                        var normalizedMatch = pattern.Substring(1);
                        normalizedMatch = normalizedMatch.Substring(0, normalizedMatch.Length - 1);
                        action.Text = action.Text.Replace(pattern, response.TemplateResolutions[normalizedMatch]);
                    }

                    recognizedPatterns = PatternRecognizer.Recognize(action.DisplayText);
                    foreach (var pattern in recognizedPatterns)
                    {
                        var normalizedMatch = pattern.Substring(1);
                        normalizedMatch = normalizedMatch.Substring(0, normalizedMatch.Length - 1);
                        action.DisplayText = action.DisplayText.Replace(pattern, response.TemplateResolutions[normalizedMatch]);
                    }
                }
            }
        }
    }
}
