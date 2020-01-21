// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    public class LuisRecognizerLoader : ICustomDeserializer
    {
        private IConfiguration configuration;

        public LuisRecognizerLoader(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            // If the luis service info is inlined with the recognizer, load it here for 
            // simpler json format
            if (obj["applicationId"]?.Type == JTokenType.String)
            {
                var luisApplication = obj.ToObject<LuisApplication>();
                luisApplication.ApplicationId = configuration.LoadSetting(luisApplication.ApplicationId);
                luisApplication.Endpoint = configuration.LoadSetting(luisApplication.Endpoint);
                luisApplication.EndpointKey = configuration.LoadSetting(luisApplication.EndpointKey);
                var options = new LuisRecognizerOptionsV3(luisApplication);
                if (obj["predictionOptions"] != null)
                {
                    options.PredictionOptions = serializer.Deserialize<AI.LuisV3.LuisPredictionOptions>(obj["predictionOptions"].CreateReader());
                }

                if (obj["externalEntityRecognizer"] != null)
                {
                    options.ExternalEntityRecognizer = serializer.Deserialize<Recognizer>(obj["externalEntityRecognizer"].CreateReader());
                }

                return new LuisRecognizer(options);
            }

            // Else, just assume it is the verbose structure with LuisService as inner object
            return obj.ToObject<LuisRecognizer>(serializer);
        }
    }
}
