using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Microsoft.Bot.Builder.Ai
{
    public class LuisRecognizerMiddleware : Middleware.IntentRecognizerMiddleware
    {
        private readonly LuisClient _luisClient;
        
        public LuisRecognizerMiddleware(string appId, string appKey) : base()
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentNullException(nameof(appId));

            if (string.IsNullOrWhiteSpace(appKey))
                throw new ArgumentNullException(nameof(appKey));

            _luisClient = new LuisClient(appId, appKey);
            SetupOnRecognize();
        }

        public LuisRecognizerMiddleware(string appId, string appKey, string baseUri) : base()
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentNullException(nameof(appId));

            if (string.IsNullOrWhiteSpace(appKey))
                throw new ArgumentNullException(nameof(appKey));

            if (string.IsNullOrWhiteSpace(baseUri))
                throw new ArgumentNullException(nameof(baseUri));

            _luisClient = new LuisClient(appId, appKey, baseUri);
            SetupOnRecognize();
        }

        private void SetupOnRecognize()
        {
            this.OnRecognize(async (context) =>
            {
                Middleware.Intent i = await RecognizeAndMap(context.Request.AsMessageActivity().Text);
                return new List<Middleware.Intent>() { i };
            });
        }
        
        private async Task<Middleware.Intent> RecognizeAndMap(string utterance)
        {
            Middleware.Intent intent = new Middleware.Intent();

            // LUIS client throws an exception on Predict is the utterance is null / empty
            // so just skip those cases and return a non-match. 
            if (string.IsNullOrWhiteSpace(utterance))
            {
                intent.Name = string.Empty;
                intent.Score = 0.0;
            }
            else
            {
                LuisResult result = await _luisClient.Predict(utterance);

                if (result.TopScoringIntent == null)
                {
                    intent.Name = string.Empty;
                    intent.Score = 0.0;
                }
                else
                {
                    intent.Name = result.TopScoringIntent.Name;
                    intent.Score = result.TopScoringIntent.Score;
                }

                foreach (var luisEntityList in result.Entities.Values)
                {
                    foreach (var luisEntity in luisEntityList)
                    {
                        intent.Entities.Add(new LuisEntity(luisEntity));
                    }
                }
            }
                        
            return intent;
        }        
    }

    public class LuisEntity : Entity
    {
        public LuisEntity()
        {
        }

        public LuisEntity(Cognitive.LUIS.Entity luisEntity)
        {
            this.Type = luisEntity.Name;
            this.Value = luisEntity.Value;
            this.StartIndex = luisEntity.StartIndex;
            this.EndIndex = luisEntity.EndIndex;
            if (luisEntity.Resolution != null)
            {
                foreach(var key in luisEntity.Resolution.Keys)
                {
                    this.Resolution[key] = luisEntity.Resolution[key];
                }
            }
        }

        public string Type { get; set; }

        public string Value { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public FlexObject Resolution { get; set; }
    }
}

