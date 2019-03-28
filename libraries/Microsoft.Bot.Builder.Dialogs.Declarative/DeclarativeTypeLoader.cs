// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Dialogs.Rules;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public static class DeclarativeTypeLoader
    {
        public static T Load<T>(string json, ResourceExplorer resourceExplorer)
        {
            IRefResolver refResolver = new IdRefResolver(resourceExplorer);

            var cog = JsonConvert.DeserializeObject<T>(
                json, new JsonSerializerSettings()
                {
                    SerializationBinder = new UriTypeBinder(),
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = new List<JsonConverter>()
                    {
                        new InterfaceConverter<IDialog>(refResolver),
                        new InterfaceConverter<IRule>(refResolver),
                        new InterfaceConverter<IStorage>(refResolver),
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

        /// <summary>
        /// Load a settings style path settings.x.y.z -> x:y:z 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string LoadSetting(this IConfiguration configuration, string value)
        {
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                var path = value.Trim('{', '}').Replace(".", ":");
                if (path.StartsWith("settings:"))
                {
                    path = path.Substring(9);
                }
                // just use configurations ability to query for x:y:z
                value = configuration.GetValue<string>(path);
            }
            return value;
        }
    }
}
