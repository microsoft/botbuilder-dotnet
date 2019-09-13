using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// EntityRecognizerSet - Implements a workflow against a pool of IEntityRecognizer instances, iterating until nobody has anything new to add.
    /// </summary>
    public class EntityRecognizerSet : List<EntityRecognizer>
    {
        public EntityRecognizerSet()
        {
        }

        public EntityRecognizerSet(IEnumerable<EntityRecognizer> recognizers)
            : base(recognizers)
        {
        }

        /// <summary>
        /// Implement RecognizeEntities by iterating against the Recognizer pool.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="entities">if no entities are passed in, it will generate a <see cref="TextEntity"/> for turnContext.Activity.Text and then generate entities off of that.</param>
        /// <returns><see cref="Entity"/> list.</returns>
        public async Task<IList<Entity>> RecognizeEntities(ITurnContext turnContext, IEnumerable<Entity> entities = null)
        {
            List<Entity> allNewEntities = new List<Entity>();
            List<Entity> entitiesToProcess = new List<Entity>(entities ?? Array.Empty<Entity>());

            if (entitiesToProcess.Count == 0)
            {
                var textEntity = new TextEntity(turnContext.Activity.Text);
                allNewEntities.Add(textEntity);
                entitiesToProcess.Add(textEntity);
            }

            do
            {
                List<Entity> newEntitiesToProcess = new List<Entity>();

                foreach (var recognizer in this)
                {
                    try
                    {
                        // get new entities
                        var newEntities = await recognizer.RecognizeEntities(turnContext, entitiesToProcess).ConfigureAwait(false);

                        foreach (var newEntity in newEntities)
                        {
                            // if unique
                            if (!allNewEntities.Any(entity => entity.Equals(newEntity)))
                            {
                                // add to all results
                                allNewEntities.Add(newEntity);

                                // add to list to be processed more
                                newEntitiesToProcess.Add(newEntity);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.TraceWarning(err.Message);
                    }
                }

                // switch to next pool of new entities to process
                entitiesToProcess = newEntitiesToProcess;
            }
            while (entitiesToProcess.Count > 0);

            return allNewEntities;
        }
    }
}
