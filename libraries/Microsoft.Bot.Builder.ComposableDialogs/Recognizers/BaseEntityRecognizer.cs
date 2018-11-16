using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.ComposableDialogs.Recognizers
{
    public abstract class BaseEntityRecognizer : IEntityRecognizer
    {
        public BaseEntityRecognizer()
        {

        }

        protected abstract List<ModelResult> Recognize(string text, string culture);

        public Task<IList<Entity>> RecognizeEntities(ITurnContext turnContext, IList<Entity> entities)
        {
            List<Entity> newEntities = new List<Entity>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var culture = Culture.MapToNearestLanguage(turnContext.Activity.Locale);

                // if there are no entities, then look at activity.Text
                if (entities.Any() == false)
                {
                    var results = Recognize(turnContext.Activity.Text, culture);
                    foreach (var number in results)
                    {
                        Entity newEntity = new Entity();
                        newEntity.SetAs(number);
                        newEntity.Type = number.TypeName;
                        newEntities.Add(newEntity);
                    }
                }
            }
            return Task.FromResult((IList<Entity>)newEntities);
        }
    }
}
