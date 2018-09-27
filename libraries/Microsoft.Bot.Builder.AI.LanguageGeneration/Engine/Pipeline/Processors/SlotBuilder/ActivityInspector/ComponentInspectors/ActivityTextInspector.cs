using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Class to inspect/search a <see cref="Activity"/> object for template references in <see cref="Activity.Text"/>. 
    /// </summary>
    internal class ActivityTextInspector : IActivityComponentInspector
    {
        /// <summary>
        /// Inspect/search a <see cref="Activity"/> object for template references in <see cref="Activity.Text"/>. 
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be searched.</param>
        /// <returns>A <see cref="IList{string}"/> containing all the referenced templates in <see cref="Activity.Text"/></returns>
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
