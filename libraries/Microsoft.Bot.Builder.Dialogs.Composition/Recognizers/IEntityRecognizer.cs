using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Recognizers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    /// <summary>
    ///  Interface for components which recognize new Entities.
    /// </summary>
    public interface IEntityRecognizer
    {
        /// <summary>
        /// Given a pool of entities and context add additional entities.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="entities">if no entities are passed in, it will generate a <see cref="TextEntity"/> for turnContext.Activity.Text and then generate entities off of that.</param>
        /// <returns>returns collection of new entities (thus triggering further evaluation).</returns>
        Task<IList<Entity>> RecognizeEntities(ITurnContext turnContext, IEnumerable<Entity> entities);
    }
}
