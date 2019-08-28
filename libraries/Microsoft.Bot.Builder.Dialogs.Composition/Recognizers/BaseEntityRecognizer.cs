using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public abstract class BaseEntityRecognizer : IEntityRecognizer
    {
        public BaseEntityRecognizer()
        {
        }

        public Task<IList<Entity>> RecognizeEntities(ITurnContext turnContext, IEnumerable<Entity> entities)
        {
            List<Entity> newEntities = new List<Entity>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var culture = Culture.MapToNearestLanguage(turnContext.Activity.Locale);

                // look for text entities to recognize 
                foreach (var entity in entities.Where(e => e.Type == TextEntity.TypeName).Select(e => e as TextEntity ?? e.GetAs<TextEntity>()))
                {
                    var results = Recognize(entity.Text, culture);
                    foreach (var result in results)
                    {
                        var newEntity = new Entity();
                        newEntity.SetAs(result);
                        newEntity.Type = result.TypeName;
                        newEntities.Add(newEntity);
                    }
                }
            }

            return Task.FromResult((IList<Entity>)newEntities);
        }

        protected abstract List<ModelResult> Recognize(string text, string culture);
    }
}
