// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader
{
    public static class CognitiveLoader
    {
        public static T Load<T>(string json)
        {
            IRefResolver refResolver = new JPointerRefResolver(JToken.Parse(json));

            var cog = JsonConvert.DeserializeObject<T>(
                json, new JsonSerializerSettings()
                {
                    SerializationBinder = new UriTypeBinder(),
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = new List<JsonConverter>()
                    {
                        new InterfaceConverter<IDialog>(refResolver),
                        new InterfaceConverter<IStep>(refResolver),
                        new StepConverter(refResolver),
                        new InterfaceConverter<IRecognizer>(refResolver),
                        new ExpressionConverter(),
                        new ActivityConverter(),
                        new ActivityTemplateConverter()
                    },
                    Error = (sender, args) =>
                    {
                        var ctx = args.ErrorContext;
                    },
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });
            return cog;
        }
    }
}
