using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for modifying <see cref="Activity"/> objects to substitute template references with their resolved values.
    /// For every possible variable in <see cref="Activity"/> a <see cref="IActivityComponentModifier"/> is created,
    /// to substitute template references within particular <see cref="Activity"/> aspect, ie: every component modifier is responsible for 
    /// modifying the <see cref="Activity"/> object from a different perspective,
    /// then the main activity modifier will loop for every configured <see cref="IActivityComponentModifier"/> and call it's modify function.
    /// </summary>
    internal interface IActivityModifier
    {
        /// <summary>
        /// Substitute a <see cref="Activity"/> for template references.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> object that will be modified ie : it's template refereces will be substituted with their resolved values.</param>
        /// <param name="response">The <see cref="ICompositeResponse"/> object that carries the tempolate resolution values, which will be used to modify the activity.</param>
        void ModifyActivity(Activity activity, ICompositeResponse response);
    }
}
