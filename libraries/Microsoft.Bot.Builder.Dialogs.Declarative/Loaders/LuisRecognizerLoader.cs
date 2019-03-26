// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                var luisService = obj.ToObject<LuisApplication>();
                luisService.ApplicationId = loadSetting(luisService.ApplicationId);
                luisService.Endpoint = loadSetting(luisService.Endpoint);
                luisService.EndpointKey = loadSetting(luisService.EndpointKey);

                return new LuisRecognizer(luisService);
            }

            // Else, just assume it is the verbose structure with LuisService as inner object
            return obj.ToObject<LuisRecognizer>(serializer);
        }

        private string loadSetting(string value)
        {
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                var path = value.Trim('{', '}').Replace(".", ":");
                // just use configurations ability to query for x:y:z
                value = configuration.GetValue<string>(path);
            }
            return value;
        }
    }
}
