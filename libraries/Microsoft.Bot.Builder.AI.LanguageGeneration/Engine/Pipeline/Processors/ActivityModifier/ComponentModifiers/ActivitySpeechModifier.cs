using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Class to modify/substitute a <see cref="Activity"/> object for template references in <see cref="Activity.Speak"/>. 
    /// </summary>
    internal class ActivitySpeechModifier : IActivityComponentModifier
    {
        /// <summary>
        /// Modify/substitute  a <see cref="Activity"/> object for template references in <see cref="Activity.Speak"/>. 
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be modified.</param>
        /// <param name="response">The <see cref="ICompositeResponse"/> object that carries the tempolate resolution values, which will be used to modify the activity.</param>
        public void Modify(Activity activity, ICompositeResponse response)
        {
            if (!string.IsNullOrWhiteSpace(activity.Speak))
            {
                var recognizedPatterns = PatternRecognizer.Recognize(activity.Speak);
                foreach (var pattern in recognizedPatterns)
                {
                    var normalizedMatch = pattern.Substring(1);
                    normalizedMatch = normalizedMatch.Substring(0, normalizedMatch.Length - 1);
                    activity.Speak = activity.Speak.Replace(pattern, response.TemplateResolutions[normalizedMatch]);
                }
            }
        }
    }
}
