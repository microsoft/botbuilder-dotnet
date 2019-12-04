// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Plugins;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Types
{
    public static class TypeFactory
    {
        private static Dictionary<Type, ICustomDeserializer> builders = new Dictionary<Type, ICustomDeserializer>();
        private static Dictionary<string, Type> types = new Dictionary<string, Type>();
        private static Dictionary<Type, string> names = new Dictionary<Type, string>();

        public static IConfiguration Configuration { get; set; }

        public static void Register(string name, Type type, ICustomDeserializer loader = null)
        {
            EnsureConfig();

            // Default loader if none specified
            if (loader == null)
            {
                loader = new DefaultLoader();
            }

            lock (types)
            {
                types[name] = type;
            }

            lock (names)
            {
                names[type] = name;
            }

            lock (builders)
            {
                builders[type] = loader;
            }
        }

        public static void RegisterPlugin(IPlugin plugin)
        {
            plugin.Load();
            Register(plugin.SchemaUri, plugin.Type, plugin.Loader);
        }

        public static T Build<T>(string name, JToken obj, JsonSerializer serializer)
            where T : class
        {
            EnsureConfig();
            ICustomDeserializer builder;
            var type = TypeFromName(name);

            if (type == null)
            {
                throw new ArgumentException($"Type {name} not registered in factory.");
            }

            var found = builders.TryGetValue(type, out builder);

            if (!found)
            {
                throw new ArgumentException($"Type {name} not registered in factory.");
            }

            var built = builder.Load(obj, serializer, type);

            var result = built as T;

            if (result == null)
            {
                throw new Exception($"Factory registration for name {name} resulted in type {built.GetType()}, but expected assignable to {typeof(T)}");
            }

            return result;
        }

        public static Type TypeFromName(string name)
        {
            Type type;
            return types.TryGetValue(name, out type) ? type : default(Type);
        }

        public static string NameFromType(Type type)
        {
            string name;
            return names.TryGetValue(type, out name) ? name : default(string);
        }

        public static void Reset()
        {
            types.Clear();
            names.Clear();
            builders.Clear();
        }

        private static void EnsureConfig()
        {
            if (TypeFactory.Configuration == null)
            {
                throw new ArgumentNullException($"TypeFactory.Configuration is not set to IConfiguration instance");
            }
        }
    }
}
