using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// TextEntityRecognizer - base class for Text.Recogizers from the text recognizer library.
    /// </summary>
    public abstract class TextEntityRecognizer : EntityRecognizer
    {
        private static JsonSerializer serializer = new JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public TextEntityRecognizer()
        {
        }

        public override Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, string text, string locale, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            List<Entity> newEntities = new List<Entity>();
            var culture = Culture.MapToNearestLanguage(locale ?? string.Empty);

            // look for text entities to recognize 
            foreach (var entity in entities.Where(e => e.Type == TextEntity.TypeName).Select(e => e as TextEntity ?? e.GetAs<TextEntity>()))
            {
                var results = Recognize(entity.Text, culture);
                foreach (var result in results)
                {
                    var newEntity = JObject.FromObject(result, serializer).ToObject<Entity>();
                    newEntity.Type = result.TypeName;
                    newEntity.Properties.Remove("TypeName");
                    newEntities.Add(newEntity);
                }
            }

            return Task.FromResult<IEnumerable<Entity>>(newEntities);
        }

        protected abstract List<ModelResult> Recognize(string text, string culture);
    }
}
