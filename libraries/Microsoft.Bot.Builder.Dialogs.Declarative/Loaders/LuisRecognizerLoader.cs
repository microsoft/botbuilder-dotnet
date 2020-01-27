// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Loaders
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
                    options.PredictionOptions = obj["predictionOptions"].ToObject<AI.LuisV3.LuisPredictionOptions>();
                }

                return new LuisRecognizer(options);
            }

            // Else, just assume it is the verbose structure with LuisService as inner object
            return obj.ToObject<LuisRecognizer>(serializer);
        }
    }
}
