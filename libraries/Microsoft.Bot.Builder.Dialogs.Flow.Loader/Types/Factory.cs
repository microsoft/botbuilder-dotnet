// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Composition;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types
{
    public static class Factory
    {
        private static Dictionary<Type, ILoader> builders = new Dictionary<Type, ILoader>();
        private static Dictionary<string, Type> types = new Dictionary<string, Type>();
        private static Dictionary<Type, string> names = new Dictionary<Type, string>();

        static Factory()
        {
            RegisterDefaults();   
        }

        public static void Register(string name, Type type, ILoader loader = null)
        {
            // Default loader if none specified
            if (loader == null)
            {
                loader = new DefaultLoader();
            }

            types.Add(name, type);
            names.Add(type, name);
            builders.Add(type, loader);
        }

        public static async Task RegisterPlugin(IPlugin plugin)
        {
            await plugin.Load();
            Register(plugin.SchemaUri, plugin.Type, plugin.Loader);
        }

        public static T Build<T>(string name, JToken obj, JsonSerializer serializer) where T : class
        {
            ILoader builder;
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
            return names.TryGetValue(type, out name) ? name: default(string);
        }

        public static void Reset()
        {
            types.Clear();
            names.Clear();
            builders.Clear();
            RegisterDefaults();
        }

        private static void RegisterDefaults()
        {
            //TODO: we don't want this static initialization, leaving it here for convenience now
            // while things are changing rapidly still

            // Steps
            Register("Microsoft.SetPropertyStep", typeof(SetPropertyStep));
            Register("Microsoft.SwitchStep", typeof(SwitchStep));
            Register("Microsoft.CallDialog", typeof(CallDialog));
            Register("Microsoft.SendActivityStep", typeof(SendActivityStep));
            Register("Microsoft.EndDialog", typeof(EndDialog));
            Register("Microsoft.ClearPropertyStep", typeof(ClearPropertyStep));
            Register("Microsoft.EndOfTurnStep", typeof(EndOfTurnStep));
            Register("Microsoft.GotoDialog", typeof(GotoDialog));
            Register("Microsoft.IfElseStep", typeof(IfElseStep));
            Register("Microsoft.SendActivityTemplateStep", typeof(SendActivityTemplateStep));

            // Dialogs
            Register("Microsoft.ComponentDialog", typeof(ComponentDialog), new ComponentDialogLoader());
            Register("Microsoft.IntentDialog", typeof(IntentDialog));
            Register("Microsoft.SequenceDialog", typeof(SequenceDialog));
            Register("Microsoft.TextPrompt", typeof(TextPrompt));
            Register("Microsoft.IntegerPrompt", typeof(IntegerPrompt));
            Register("Microsoft.FloatPrompt", typeof(FloatPrompt));

            // Recognizers
            Register("Microsoft.LuisRecognizer", typeof(LuisRecognizer), new LuisRecognizerLoader());
        }
    }
}
