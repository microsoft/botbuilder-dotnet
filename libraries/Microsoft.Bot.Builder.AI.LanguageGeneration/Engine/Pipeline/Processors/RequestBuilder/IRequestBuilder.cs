using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for building the <see cref="ICompositeRequest"/> object, which in turn contains all the unique template refereces in user <see cref="Activity"/> and the <see cref="Slot"/> objets that contains user entity values.
    /// </summary>
    internal interface IRequestBuilder
    {
        /// <summary>
        /// The main method to build the <see cref="ICompositeRequest"/> object.
        /// </summary>
        /// <param name="slots">The <see cref="IList{Slot}"/>.</param>
        /// <param name="locale">Locale.</param>
        /// <returns>A <see cref="ICompositeRequest"/>.</returns>
        ICompositeRequest BuildRequest(IList<Slot> slots, string locale);
    }
}
