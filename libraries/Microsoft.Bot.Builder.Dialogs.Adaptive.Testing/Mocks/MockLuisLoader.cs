// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis.Testing
{
    public class MockLuisLoader : ICustomDeserializer
    {
        public MockLuisLoader()
        {
        }

        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            var recognizer = obj.ToObject<LuisAdaptiveRecognizer>(serializer);
            var name = recognizer.ApplicationId.ToString();
            if (name.StartsWith("="))
            {
                var start = name.LastIndexOf('.') + 1;
                name = name.Substring(start);
            }

            var configuration = HostContext.Current.Get<IConfiguration>();
            return new MockLuisRecognizer(recognizer, configuration.GetValue<string>("luis:resources"), name);
        }
    }
}
