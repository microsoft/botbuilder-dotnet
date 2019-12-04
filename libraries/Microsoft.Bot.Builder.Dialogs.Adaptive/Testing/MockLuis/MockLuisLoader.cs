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
                var luisService = obj.ToObject<LuisApplication>();
                var name = luisService.ApplicationId;
                if (name.StartsWith("{") && name.EndsWith("}"))
                {
                    var start = name.LastIndexOf('.') + 1;
                    var end = name.LastIndexOf('}');
                    name = name.Substring(start, end - start);
                }

                luisService.ApplicationId = configuration.LoadSetting(luisService.ApplicationId);
                luisService.Endpoint = configuration.LoadSetting(luisService.Endpoint);
                luisService.EndpointKey = configuration.LoadSetting(luisService.EndpointKey);

                return new MockLuisRecognizer(luisService, configuration.GetValue<string>("luis:resources"), name);
            }

            // Else, just assume it is the verbose structure with LuisService as inner object
            return obj.ToObject<LuisRecognizer>(serializer);
        }
    }
}
