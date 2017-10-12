using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai
{
    public class LuisRecognizerMiddleware : IntentRecognizerMiddleware
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
                Intent i = await RecognizeAndMap(context.Request.Text);
                return new List<Intent>() { i };
            });
        }
        
        private async Task<Intent> RecognizeAndMap(string utterance)
        {
            utterance = (string.IsNullOrWhiteSpace(utterance)) ? string.Empty : utterance;
            LuisResult result = await _luisClient.Predict(utterance);

            Intent intent = new Intent();
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

        public string Type
        {
            get { return this["type"]; }
            set { this["type"] = value; }
        }

        public string Value
        {
            get { return this["value"]; }
            set { this["value"] = value; }
        }

        public int StartIndex
        {
            get { return this["startIndex"]; }
            set { this["startIndex"] = value; }
        }

        public int EndIndex
        {
            get { return this["endIndex"]; }
            set { this["endIndex"] = value; }
        }

        public FlexObject Resolution
        {
            get { return this["resultion"]; }
            set { this["resolution"] = value; }
        }
    }
}

