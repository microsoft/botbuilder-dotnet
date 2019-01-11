using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Composition;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders;
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
            //TODO: we don't want this static initialization, leaving it here for convenience now
            // while things are changing rapidly still

            // Commands
            Register("http://schemas.botframework.com/SetVariable", typeof(SetVariable), new SetVariableCommandLoader());
            Register("http://schemas.botframework.com/Switch", typeof(Switch));
            Register("http://schemas.botframework.com/CallDialog", typeof(CallDialog));
            Register("http://schemas.botframework.com/SendActivity", typeof(SendActivity));
            Register("http://schemas.botframework.com/CommandSet", typeof(CommandSet));

            // Dialogs
            Register("http://schemas.botframework.com/ComponentDialog", typeof(ComponentDialog), new ComponentDialogLoader());
            Register("http://schemas.botframework.com/CommandDialog", typeof(CommandDialog), new CommandDialogLoader());
            Register("http://schemas.botframework.com/IntentCommandDialog", typeof(IntentCommandDialog), new IntentCommandDialogLoader());
            Register("http://schemas.botframework.com/IntentDialog", typeof(IntentDialog));
            Register("http://schemas.botframework.com/TextPrompt", typeof(TextPrompt));
            Register("http://schemas.botframework.com/IntNumberPrompt", typeof(NumberPrompt<Int32>));

            // Recognizers
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

        public static T Build<T>(string name, JObject obj, JsonSerializer serializer) where T : class
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

        // Plugins
        //public static void Register<T>(string friendlyName, Type dotnetType, Assembly assembly, Func<JsonReader, JsonSerializer, T> builder)
        //{

        //}
    }
}
