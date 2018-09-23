using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for modifying/substituting <see cref="Activity"/> for template references according to a specific criteria ie: modify activity from a particular perspective,
    /// for example a class that implements the <see cref="IActivityComponentModifier"/> could modify/substitute the activity for template references in <see cref="Activity.Text"/>.
    /// </summary>
    internal interface IActivityComponentModifier
    {
        /// <summary>
        /// Modifies/substitutes the <see cref="Activity"/> from a particular perspective.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be modified.</param>
        /// <param name="response">The <see cref="ICompositeResponse"/> object that carries the tempolate resolution values, which will be used to modify the activity.</param>
        void Modify(Activity activity, ICompositeResponse response);
    }
}
