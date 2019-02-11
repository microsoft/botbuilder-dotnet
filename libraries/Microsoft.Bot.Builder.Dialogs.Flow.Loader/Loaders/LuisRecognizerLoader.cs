// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    public class LuisRecognizerLoader : ICustomDeserializer
    {
        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            // If the luis service info is inlined with the recognizer, load it here for 
            // simpler json format
            if (obj["applicationId"]?.Type == JTokenType.String)
            {
                var luisService = obj.ToObject<LuisApplication>();
                return new LuisRecognizer(luisService);
            }

            // Else, just assume it is the verbose structure with LuisService as inner object
            return obj.ToObject<LuisRecognizer>(serializer);
        }
    }
}
