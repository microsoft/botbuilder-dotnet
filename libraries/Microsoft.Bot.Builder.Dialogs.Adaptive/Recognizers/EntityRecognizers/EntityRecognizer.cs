using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public abstract class EntityRecognizer
    {
        public EntityRecognizer()
        {
        }

        public Task<IList<Entity>> RecognizeEntities(DialogContext dialogContext, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            return this.RecognizeEntities(dialogContext, dialogContext.Context.Activity, entities, cancellationToken);
        }

        public async Task<IList<Entity>> RecognizeEntities(DialogContext dialogContext, Activity activity, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                return await this.RecognizeEntities(dialogContext, activity.Text, activity.Locale, entities, cancellationToken).ConfigureAwait(false);
            }

            return new List<Entity>();
        }

        public Task<IList<Entity>> RecognizeEntities(DialogContext dialogContext, string text, string locale, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            List<Entity> newEntities = new List<Entity>();
            var culture = Culture.MapToNearestLanguage(locale ?? string.Empty);

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

            return Task.FromResult((IList<Entity>)newEntities);
        }

        protected abstract List<ModelResult> Recognize(string text, string culture);
    }
}
