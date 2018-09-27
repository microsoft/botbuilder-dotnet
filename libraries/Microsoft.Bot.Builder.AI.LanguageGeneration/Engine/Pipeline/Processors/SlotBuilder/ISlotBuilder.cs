using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Builder responsible for building <see cref="Slot"/> objects which are the main data container carrying user referenced templates that need to be resolved.
    /// </summary>
    internal interface ISlotBuilder
    {
        /// <summary>
        /// The builder executor function, takes a <see cref="Activity"/> and a <see cref="IDictionary{string, object}"/> and builds a list of <see cref="Slot"/> objects that will be used to carry data to the service.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> containing template references that need to be resolved.</param>
        /// <param name="entities">The list containing entity values that will be used to substitute entity references in template resolution values.</param>
        /// <returns>a <see cref="IList{Slot}"/></returns>
        IList<Slot> BuildSlots(Activity activity, IDictionary<string, object> entities);
    }
}
