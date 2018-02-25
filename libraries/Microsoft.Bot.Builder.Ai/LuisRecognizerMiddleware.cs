// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;
using Intent = Microsoft.Bot.Builder.Middleware.Intent;

namespace Microsoft.Bot.Builder.Ai
{
    public class LuisRecognizerMiddleware : Middleware.IntentRecognizerMiddleware
    {
        private readonly LuisClient _luisClient;
        
        public LuisRecognizerMiddleware(string appId, string appKey, bool endActivityRoutingOnIntentHandled = true) : base(endActivityRoutingOnIntentHandled)
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentNullException(nameof(appId));

            if (string.IsNullOrWhiteSpace(appKey))
                throw new ArgumentNullException(nameof(appKey));

            _luisClient = new LuisClient(appId, appKey);
            SetupOnRecognize();
        }

        public LuisRecognizerMiddleware(string appId, string appKey, string baseUri, bool endActivityRoutingOnIntentHandled = true) : base(endActivityRoutingOnIntentHandled)
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
                return await RecognizeAndMap(context.Request.AsMessageActivity()?.Text);
            });
        }
        
        private async Task<List<Middleware.Intent>> RecognizeAndMap(string utterance)
        {
            List<Middleware.Intent> intents = new List<Middleware.Intent>();

            // LUIS client throws an exception on Predict is the utterance is null / empty
            // so just skip those cases and return a non-match. 
            if (string.IsNullOrWhiteSpace(utterance))
            {
                return new List<Intent>();
            }
            else
            {
                LuisResult result = await _luisClient.Predict(utterance);

                foreach (var resultIntent in result.Intents)
                {
                    var intent = new Intent()
                    {
                        Name = resultIntent.Name,
                        Score = resultIntent.Score
                    };

                    foreach (var luisEntityList in result.Entities.Values)
                    {
                        foreach (var luisEntity in luisEntityList)
                        {
                            intent.Entities.Add(new LuisEntity(luisEntity));
                        }
                    }

                    intents.Add(intent);
                }
            }
                        
            return intents;
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
            this.Resolution = new FlexObject();
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

