using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Class to inspect/search a <see cref="Activity"/> object for template references in <see cref="Activity.Speak"/>. 
    /// </summary>
    internal class ActivitySpeechInspector : IActivityComponentInspector
    {
        /// <summary>
        /// Inspect/search a <see cref="Activity"/> object for template references in <see cref="Activity.Speak"/>. 
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be searched.</param>
        /// <returns>A <see cref="IList{string}"/> containing all the referenced templates in <see cref="Activity.Speak"/>.</returns>
        public IList<string> Inspect(Activity activity)
        {
            if (string.IsNullOrWhiteSpace(activity.Speak))
            {
                return new List<string>();
            }
            else
            {
                return PatternRecognizer.Recognize(activity.Speak);
            }
        }
    }
}
