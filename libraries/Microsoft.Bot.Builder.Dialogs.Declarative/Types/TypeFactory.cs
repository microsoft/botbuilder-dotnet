// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Events;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
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

            lock (names)
            {
                if (!names.ContainsKey(type))
                {
                    names.Add(type, name);
                }
            }

            lock (builders)
            {
                if (!builders.ContainsKey(type))
                {
                    builders.Add(type, loader);
                }
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
            EnsureConfig();
            types.Clear();
            names.Clear();
            builders.Clear();
            RegisterAdaptiveTypes();
        }

        public static void RegisterAdaptiveTypes()
        {
            EnsureConfig();

            // TODO: we don't want this static initialization, leaving it here for convenience now
            // while things are changing rapidly still

            // Events
            Register("Microsoft.OnDialogEvent", typeof(Adaptive.Events.OnDialogEvent));
            Register("Microsoft.OnIntent", typeof(OnIntent));
            Register("Microsoft.OnUnknownIntent", typeof(OnUnknownIntent));
            Register("Microsoft.OnBeginDialog", typeof(OnBeginDialog));
            Register("Microsoft.OnActivity", typeof(OnActivity));
            Register("Microsoft.OnMessageActivity", typeof(OnMessageActivity));
            Register("Microsoft.OnMessageUpdateActivity", typeof(OnMessageUpdateActivity));
            Register("Microsoft.OnMessageDeleteActivity", typeof(OnMessageDeleteActivity));
            Register("Microsoft.OnMessageReactionActivity", typeof(OnMessageReactionActivity));
            Register("Microsoft.OnEventActivity", typeof(OnEventActivity));
            Register("Microsoft.OnInvokeActivity", typeof(OnInvokeActivity));
            Register("Microsoft.OnConversationUpdateActivity", typeof(OnConversationUpdateActivity));
            Register("Microsoft.OnEndOfConversationActivity", typeof(OnEndOfConversationActivity));
            Register("Microsoft.OnTypingActivity", typeof(OnTypingActivity));
            Register("Microsoft.OnHandoffActivity", typeof(OnHandoffActivity));

            // Actions
            Register("Microsoft.BeginDialog", typeof(BeginDialog));
            Register("Microsoft.CancelAllDialogs", typeof(CancelAllDialogs));
            Register("Microsoft.DebugBreak", typeof(DebugBreak));
            Register("Microsoft.DeleteProperty", typeof(DeleteProperty));
            Register("Microsoft.EditArray", typeof(EditArray));
            Register("Microsoft.EditActions", typeof(EditActions));
            Register("Microsoft.EmitEvent", typeof(EmitEvent));
            Register("Microsoft.EndDialog", typeof(EndDialog));
            Register("Microsoft.EndTurn", typeof(EndTurn));
            Register("Microsoft.Foreach", typeof(Foreach));
            Register("Microsoft.ForeachPage", typeof(ForeachPage));
            Register("Microsoft.HttpRequest", typeof(HttpRequest));
            Register("Microsoft.IfCondition", typeof(IfCondition));
            Register("Microsoft.InitProperty", typeof(InitProperty));
            Register("Microsoft.LogAction", typeof(LogAction));
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
            Register("Microsoft.IntentPattern", typeof(IntentPattern));
            Register("Microsoft.MultiLanguageRecognizer", typeof(MultiLanguageRecognizer));

            // Entity recognizers
            Register("Microsoft.AgeEntityRecognizer", typeof(AgeEntityRecognizer));
            Register("Microsoft.ConfirmationEntityRecognizer", typeof(ConfirmationEntityRecognizer));
            Register("Microsoft.CurrencyEntityRecognizer", typeof(CurrencyEntityRecognizer));
            Register("Microsoft.DateTimeEntityRecognizer", typeof(DateTimeEntityRecognizer));
            Register("Microsoft.DimensionEntityRecognizer", typeof(DimensionEntityRecognizer));
            Register("Microsoft.EmailEntityRecognizer", typeof(EmailEntityRecognizer));
            Register("Microsoft.EntityRecognizer", typeof(EntityRecognizer));
            Register("Microsoft.EntityRecognizerSet", typeof(EntityRecognizerSet));
            Register("Microsoft.GuidEntityRecognizer", typeof(GuidEntityRecognizer));
            Register("Microsoft.HashtagEntityRecognizer", typeof(HashtagEntityRecognizer));
            Register("Microsoft.IpEntityRecognizer", typeof(IpEntityRecognizer));
            Register("Microsoft.MentionEntityRecognizer", typeof(MentionEntityRecognizer));
            Register("Microsoft.NumberEntityRecognizer", typeof(NumberEntityRecognizer));
            Register("Microsoft.NumberRangeEntityRecognizer", typeof(NumberRangeEntityRecognizer));
            Register("Microsoft.OrdinalEntityRecognizer", typeof(OrdinalEntityRecognizer));
            Register("Microsoft.PercentageEntityRecognizer", typeof(PercentageEntityRecognizer));
            Register("Microsoft.PhoneNumberEntityRecognizer", typeof(PhoneNumberEntityRecognizer));
            Register("Microsoft.TemperatureEntityRecognizer", typeof(TemperatureEntityRecognizer));
            Register("Microsoft.UrlEntityRecognizer", typeof(UrlEntityRecognizer));

            // Dialogs
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
