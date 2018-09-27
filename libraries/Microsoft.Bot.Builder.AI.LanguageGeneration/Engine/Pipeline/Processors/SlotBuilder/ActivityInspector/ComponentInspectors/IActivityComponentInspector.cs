using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for inspecting <see cref="Activity"/> for template references according to a specific criteria ie: search activity from a particular perspective,
    /// for example a class that implements the <see cref="IActivityComponentInspector"/> could search the activity for template references in <see cref="Activity.Text"/>.
    /// </summary>
    internal interface IActivityComponentInspector
    {
        /// <summary>
        /// Searches the <see cref="Activity"/> from a particular perspective.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be searched.</param>
        ///<returns>A <see cref="IList{string}"/> containing all the referenced templates in activity according to the searching criteria,
        /// which will be defined in each concrete class that implements the <see cref="IActivityComponentInspector"/> interface.</returns>
        IList<string> Inspect(Activity activity);
    }
}
