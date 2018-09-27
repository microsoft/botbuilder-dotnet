using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for inspecting <see cref="Activity"/> objects for template references.
    /// For every possible variable in <see cref="Activity"/> a <see cref="IActivityComponentInspector"/> is created,
    /// to check for template references within particular <see cref="Activity"/> aspect, ie: every component inspector is responsible for 
    /// searching the <see cref="Activity"/> object from a different perspective,
    /// then the main activity inspector will loop for every configured <see cref="IActivityComponentInspector"/> and call it's inspect function.
    /// </summary>
    internal interface IActivityInspector
    {
        /// <summary>
        /// Inspects/Searches a <see cref="Activity"/> for template references.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> object that will be inspected/searched for template refereces .</param>
        /// <returns>a <see cref="IList{string}"/> containing recognized template references.</returns>
        IList<string> Inspect(Activity activity);
    }
}
