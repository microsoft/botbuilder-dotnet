using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    /// <summary>
    ///  Interface for components which recognize new Entities.
    /// </summary>
    public interface IEntityRecognizer
    {
        /// <summary>
        /// RecognizerEntities() - given a pool of entities and context add additional entities.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="entities"></param>
        /// <returns>returns collection of new entities (thus triggering further evaluation).</returns>
        Task<IList<Entity>> RecognizeEntities(ITurnContext turnContext, IEnumerable<Entity> entities);
    }
}
