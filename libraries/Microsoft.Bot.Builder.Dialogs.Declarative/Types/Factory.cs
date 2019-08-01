// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
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
                if (!types.ContainsKey(name))
                {
                    types.Add(name, type);
                }
            }
            lock(names)
            {
                if (!names.ContainsKey(type))
                {
                    names.Add(type, name);
                }
            }

            lock(builders)
            {
                if (!builders.ContainsKey(type))
                {
                    builders.Add(type, loader);
                }
            }
        }

        public static async Task RegisterPlugin(IPlugin plugin)
        {
            await plugin.Load();
            Register(plugin.SchemaUri, plugin.Type, plugin.Loader);
        }

        public static T Build<T>(string name, JToken obj, JsonSerializer serializer) where T : class
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
            EnsureConfig();
            types.Clear();
            names.Clear();
            builders.Clear();
            RegisterAdaptiveTypes();
        }

        public static void RegisterAdaptiveTypes()
        {
            EnsureConfig();

            //TODO: we don't want this static initialization, leaving it here for convenience now
            // while things are changing rapidly still

            // Rules
            Register("Microsoft.EventRule", typeof(EventRule));
            Register("Microsoft.IntentRule", typeof(IntentRule));
            Register("Microsoft.UnknownIntentRule", typeof(UnknownIntentRule));

            Register("Microsoft.ActivityRule", typeof(ActivityRule));
            Register("Microsoft.MessageActivityRule", typeof(MessageActivityRule));
            Register("Microsoft.MessageUpdateActivityRule", typeof(MessageUpdateActivityRule));
            Register("Microsoft.MessageDeleteActivityRule", typeof(MessageDeleteActivityRule));
            Register("Microsoft.MessageReactionActivityRule", typeof(MessageReactionActivityRule));
            Register("Microsoft.EventActivityRule", typeof(EventActivityRule));
            Register("Microsoft.InvokeActivityRule", typeof(InvokeActivityRule));
            Register("Microsoft.ConversationUpdateActivityRule", typeof(ConversationUpdateActivityRule));
            Register("Microsoft.EndOfConversationActivityRule", typeof(EndOfConversationActivityRule));
            Register("Microsoft.TypingActivityRule", typeof(TypingActivityRule));
            Register("Microsoft.HandoffActivityRule", typeof(HandoffActivityRule));

            // Steps
            Register("Microsoft.BeginDialog", typeof(BeginDialog));
            Register("Microsoft.CancelAllDialogs", typeof(CancelAllDialogs));
            Register("Microsoft.DebugBreak", typeof(DebugBreak));
            Register("Microsoft.DeleteProperty", typeof(DeleteProperty));
            Register("Microsoft.EditArray", typeof(EditArray));
            Register("Microsoft.EditSteps", typeof(EditSteps));
            Register("Microsoft.EmitEvent", typeof(EmitEvent));
            Register("Microsoft.EndDialog", typeof(EndDialog));
            Register("Microsoft.EndTurn", typeof(EndTurn));
            Register("Microsoft.Foreach", typeof(Foreach));
            Register("Microsoft.ForeachPage", typeof(ForeachPage));
            Register("Microsoft.HttpRequest", typeof(HttpRequest));
            Register("Microsoft.IfCondition", typeof(IfCondition));
            Register("Microsoft.InitProperty", typeof(InitProperty));
            Register("Microsoft.LogStep", typeof(LogStep));
            Register("Microsoft.RepeatDialog", typeof(RepeatDialog));
            Register("Microsoft.ReplaceDialog", typeof(ReplaceDialog));
            Register("Microsoft.SendActivity", typeof(SendActivity));
            Register("Microsoft.SetProperty", typeof(SetProperty));
            Register("Microsoft.SwitchCondition", typeof(SwitchCondition));
            Register("Microsoft.TraceActivity", typeof(TraceActivity));

            // Inputs
            Register("Microsoft.AttachmentInput", typeof(AttachmentInput));
            Register("Microsoft.ConfirmInput", typeof(ConfirmInput));
            Register("Microsoft.NumberInput", typeof(NumberInput));
            Register("Microsoft.TextInput", typeof(TextInput));
            Register("Microsoft.ChoiceInput", typeof(ChoiceInput));
            Register("Microsoft.DateTimeInput", typeof(DateTimeInput));
            Register("Microsoft.OAuthInput", typeof(OAuthInput));

            // Recognizers
            Register("Microsoft.LuisRecognizer", typeof(LuisRecognizer), new LuisRecognizerLoader(TypeFactory.Configuration));
            Register("Microsoft.RegexRecognizer", typeof(RegexRecognizer));
            Register("Microsoft.MultiLanguageRecognizer", typeof(MultiLanguageRecognizer));

            Register("Microsoft.AdaptiveDialog", typeof(AdaptiveDialog));
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
