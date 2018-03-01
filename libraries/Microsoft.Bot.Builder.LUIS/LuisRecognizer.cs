using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;
using Microsoft.Cognitive.LUIS.Models;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.LUIS
{
    /// <summary>
    /// A recognizer based on LUIS
    /// </summary>
    public class LuisRecognizer : IRecognizer
    {
        private readonly LuisService _luisService;
        private readonly ILuisOptions _luisOptions;

        public LuisRecognizer(ILuisModel luisModel, ILuisOptions options = null)
        {
            _luisService = new LuisService(luisModel);
            _luisOptions = options;
        }

        public Task<RecognizerResult> Recognize(string utterance, CancellationToken ct, bool verbose)
        {
            var luisRequest = new LuisRequest(utterance);
            _luisOptions?.Apply(luisRequest);
            return Recognize(luisRequest, ct, verbose);
        }

        private async Task<RecognizerResult> Recognize(LuisRequest request, CancellationToken ct, bool verbose)
        {
            var luisResult = await _luisService.QueryAsync(request, ct);

            var recognizerResult = new RecognizerResult
            {
                Text = request.Query,
                Intents = GetIntents(luisResult),
                Entities = GetEntitiesAndMetadata(luisResult.Entities, luisResult.CompositeEntities, verbose)
            };

            return recognizerResult;
        }

        private static JObject GetIntents(LuisResult luisResult)
        {
            return luisResult.Intents != null ?
                JObject.FromObject(luisResult.Intents.ToDictionary(i => i.Intent, i => i.Score ?? 0)) :
                new JObject { [luisResult.TopScoringIntent.Intent] = luisResult.TopScoringIntent.Score ?? 0 };
        }

        private static JObject GetEntitiesAndMetadata(IList<EntityRecommendation> entities, IList<CompositeEntity> compositeEntities, bool verbose)
        {
            var entitiesAndMetadata = new JObject();
            if (verbose)
            {
                entitiesAndMetadata["$instance"] = new JObject();
            }
            var compositeEntityTypes = new HashSet<string>();

            // We start by populating composite entities so that entities covered by them are removed from the entities list
            if (compositeEntities != null && compositeEntities.Any())
            {
                compositeEntityTypes = new HashSet<string>(compositeEntities.Select(ce => ce.ParentType));
                foreach (var compositeEntity in compositeEntities)
                {
                    entities = PopulateCompositeEntity(compositeEntity, entities, entitiesAndMetadata, verbose);
                }
            }

            foreach (var entity in entities)
            {
                // we'll address composite entities separately
                if (compositeEntityTypes.Contains(entity.Type))
                    continue;

                AddProperty(entitiesAndMetadata, GetNormalizedEntityType(entity), GetEntityValue(entity));

                if (verbose)
                {
                    AddProperty((JObject) entitiesAndMetadata["$instance"], GetNormalizedEntityType(entity), GetEntityMetadata(entity));
                }
            }

            return entitiesAndMetadata;
        }

        private static JToken GetEntityValue(EntityRecommendation entity)
        {
            if (entity.Type.StartsWith("builtin.datetimeV2."))
            {
                return new JValue(entity.Resolution?.Values != null && entity.Resolution.Values.Count > 0
                            ? ((IDictionary<string, object>)((IList<object>)entity.Resolution.Values.First()).First())["timex"]
                            : entity.Resolution);
            }
            if (entity.Resolution != null)
            {
                if (entity.Type.StartsWith("builtin.number"))
                {
                    var value = (string)entity.Resolution.Values.First();
                    return int.TryParse(value, out var intVal) ? new JValue(intVal) : new JValue(double.Parse(value));
                }
            }
            return entity.Entity;
        }

        private static JObject GetEntityMetadata(EntityRecommendation entity)
        {
            return JObject.FromObject(new
            {
                startIndex = entity.StartIndex,
                endIndex = entity.EndIndex,
                text = entity.Entity,
                score = entity.Score
            });
        }

        private static string GetNormalizedEntityType(EntityRecommendation entity)
        {
            return Regex.Replace(entity.Type, "\\.", "_");
        }

        private static IList<EntityRecommendation> PopulateCompositeEntity(CompositeEntity ce, IList<EntityRecommendation> entities, JObject entitiesAndMetadata, bool verbose)
        {
            return entities;
        }

        /**
         * If a property doesn't exist add it to a new array, otherwise append it to the existing array
         * @param obj Object on which the property is to be set
         * @param key Property Key
         * @param value Property Value
         */
        private static void AddProperty(JObject obj, string key, JToken value)
        {
            if (obj.ContainsKey(key))
            {
                ((JArray) obj[key]).Add(value);
            }
            else
            {
                obj[key] = new JArray(value);
            }
        }
    }
}
