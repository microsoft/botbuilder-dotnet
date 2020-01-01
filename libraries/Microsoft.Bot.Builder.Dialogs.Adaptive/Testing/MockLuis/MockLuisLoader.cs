// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.MockLuis
{
    public class MockLuisLoader : ICustomDeserializer
    {
        private IConfiguration configuration;

        public MockLuisLoader(IConfiguration configuration)
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
                var name = luisApplication.ApplicationId;
                if (name.StartsWith("{") && name.EndsWith("}"))
                {
                    var start = name.LastIndexOf('.') + 1;
                    var end = name.LastIndexOf('}');
                    name = name.Substring(start, end - start);
                }   
                
                luisApplication.ApplicationId = configuration.LoadSetting(luisApplication.ApplicationId);
                luisApplication.Endpoint = configuration.LoadSetting(luisApplication.Endpoint);
                luisApplication.EndpointKey = configuration.LoadSetting(luisApplication.EndpointKey);                

                var options = new LuisRecognizerOptionsV3(luisApplication);
                if (obj["predictionOptions"] != null)
                {
                    options.PredictionOptions = obj["predictionOptions"].ToObject<AI.LuisV3.LuisPredictionOptions>();
                }

                return new MockLuisRecognizer(options, configuration.GetValue<string>("luis:resources"), name);
            }

            // Else, just assume it is the verbose structure with LuisService as inner object
            return obj.ToObject<MockLuisRecognizer>(serializer);
        }
    }
}
