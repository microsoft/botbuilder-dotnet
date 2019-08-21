// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public static class DeclarativeTypeLoader
    {
        public static async Task<T> LoadAsync<T>(IResource resource, ResourceExplorer resourceExplorer, Source.IRegistry registry)
        {
            IRefResolver refResolver = new IdRefResolver(resourceExplorer, registry);

            string id = resource.Id;
            var paths = new Stack<string>();
            if (resource is FileResource fileResource)
            {
                id = fileResource.FullName;
                paths.Push(fileResource.FullName);
            }

            try
            {
                var json = await resource.ReadTextAsync();

                return _load<T>(registry, refResolver, paths, json);
            }
            catch (Exception err)
            {
                if (err.InnerException is SyntaxErrorException)
                {
                    throw new SyntaxErrorException(err.InnerException.Message)
                    {
                        Source = $"{id}{err.InnerException.Source}"
                    };
                }

                throw new Exception($"{id} error: {err.Message}\n{err.InnerException?.Message}");
            }
        }

        public static T Load<T>(IResource resource, ResourceExplorer resourceExplorer, Source.IRegistry registry)
        {
            return LoadAsync<T>(resource, resourceExplorer, registry).GetAwaiter().GetResult();
        }

        private static T _load<T>(Source.IRegistry registry, IRefResolver refResolver, Stack<string> paths, string json)
        {
            return JsonConvert.DeserializeObject<T>(
                json, new JsonSerializerSettings()
                {
                    SerializationBinder = new UriTypeBinder(),
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = new List<JsonConverter>()
                    {
                        new InterfaceConverter<IDialog>(refResolver, registry, paths),
                        new InterfaceConverter<IOnEvent>(refResolver, registry, paths),
                        new InterfaceConverter<IStorage>(refResolver, registry, paths),
                        new InterfaceConverter<IRecognizer>(refResolver, registry, paths),
                        new LanguageGeneratorConverter(refResolver, registry, paths),
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
                value = ConfigurationBinder.GetValue<string>(configuration, path);
            }
            return value;
        }
    }
}
