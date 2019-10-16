// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private static List<ComponentRegistration> components = new List<ComponentRegistration>();

        public static void AddComponent(ComponentRegistration component)
        {
            if (!components.Any(c => c.GetType() == component.GetType()))
            {
                components.Add(component);

                foreach (var typeRegistration in component.GetTypes())
                {
                    TypeFactory.Register(typeRegistration.Name, typeRegistration.Type, typeRegistration.CustomDeserializer);
                }
            }
        }

        public static async Task<T> LoadAsync<T>(IResource resource, ResourceExplorer resourceExplorer, ISourceMap sourceMap)
        {
            IRefResolver refResolver = new IdRefResolver(resourceExplorer, sourceMap);

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

                return Load<T>(sourceMap, refResolver, paths, json);
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

        public static T Load<T>(IResource resource, ResourceExplorer resourceExplorer, ISourceMap sourceMap)
        {
            return LoadAsync<T>(resource, resourceExplorer, sourceMap).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Load a settings style path settings.x.y.z -> x:y:z. 
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="value">Value to load.</param>
        /// <returns>The value formatted to the configuration.</returns>
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

        private static T Load<T>(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths, string json)
        {
            var converters = new List<JsonConverter>();
            foreach (var component in components)
            {
                var result = component.GetConverters(sourceMap, refResolver, paths);
                if (result.Any())
                {
                    converters.AddRange(result);
                }
            }

            return JsonConvert.DeserializeObject<T>(
                json, new JsonSerializerSettings()
                {
                    SerializationBinder = new UriTypeBinder(),
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = converters,
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
    }
}
