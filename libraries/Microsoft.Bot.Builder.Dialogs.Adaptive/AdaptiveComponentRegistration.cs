using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class AdaptiveComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            // Conditionals
            yield return new TypeRegistration<OnCondition>("Microsoft.OnCondition");
            yield return new TypeRegistration<OnError>("Microsoft.OnError");

            yield return new TypeRegistration<OnDialogEvent>("Microsoft.OnDialogEvent");
            yield return new TypeRegistration<OnCustomEvent>("Microsoft.OnCustomEvent");

            yield return new TypeRegistration<OnBeginDialog>("Microsoft.OnBeginDialog");
            yield return new TypeRegistration<OnCancelDialog>("Microsoft.OnCancelDialog");
            yield return new TypeRegistration<OnRepromptDialog>("Microsoft.OnRepromptDialog");

            yield return new TypeRegistration<OnIntent>("Microsoft.OnIntent");
            yield return new TypeRegistration<OnUnknownIntent>("Microsoft.OnUnknownIntent");

            yield return new TypeRegistration<OnActivity>("Microsoft.OnActivity");
            yield return new TypeRegistration<OnMessageActivity>("Microsoft.OnMessageActivity");
            yield return new TypeRegistration<OnMessageUpdateActivity>("Microsoft.OnMessageUpdateActivity");
            yield return new TypeRegistration<OnMessageDeleteActivity>("Microsoft.OnMessageDeleteActivity");
            yield return new TypeRegistration<OnMessageReactionActivity>("Microsoft.OnMessageReactionActivity");
            yield return new TypeRegistration<OnEventActivity>("Microsoft.OnEventActivity");
            yield return new TypeRegistration<OnInvokeActivity>("Microsoft.OnInvokeActivity");
            yield return new TypeRegistration<OnConversationUpdateActivity>("Microsoft.OnConversationUpdateActivity");
            yield return new TypeRegistration<OnEndOfConversationActivity>("Microsoft.OnEndOfConversationActivity");
            yield return new TypeRegistration<OnTypingActivity>("Microsoft.OnTypingActivity");
            yield return new TypeRegistration<OnHandoffActivity>("Microsoft.OnHandoffActivity");

            // Actions
            yield return new TypeRegistration<BeginDialog>("Microsoft.BeginDialog");
            yield return new TypeRegistration<CancelAllDialogs>("Microsoft.CancelAllDialogs");
            yield return new TypeRegistration<DebugBreak>("Microsoft.DebugBreak");
            yield return new TypeRegistration<DeleteProperty>("Microsoft.DeleteProperty");
            yield return new TypeRegistration<EditArray>("Microsoft.EditArray");
            yield return new TypeRegistration<EditActions>("Microsoft.EditActions");
            yield return new TypeRegistration<EmitEvent>("Microsoft.EmitEvent");
            yield return new TypeRegistration<EndDialog>("Microsoft.EndDialog");
            yield return new TypeRegistration<EndTurn>("Microsoft.EndTurn");
            yield return new TypeRegistration<Foreach>("Microsoft.Foreach");
            yield return new TypeRegistration<ForeachPage>("Microsoft.ForeachPage");
            yield return new TypeRegistration<HttpRequest>("Microsoft.HttpRequest");
            yield return new TypeRegistration<IfCondition>("Microsoft.IfCondition");
            yield return new TypeRegistration<InitProperty>("Microsoft.InitProperty");
            yield return new TypeRegistration<LogAction>("Microsoft.LogAction");
            yield return new TypeRegistration<RepeatDialog>("Microsoft.RepeatDialog");
            yield return new TypeRegistration<ReplaceDialog>("Microsoft.ReplaceDialog");
            yield return new TypeRegistration<SendActivity>("Microsoft.SendActivity");
            yield return new TypeRegistration<SetProperty>("Microsoft.SetProperty");
            yield return new TypeRegistration<SwitchCondition>("Microsoft.SwitchCondition");
            yield return new TypeRegistration<TraceActivity>("Microsoft.TraceActivity");

            // Inputs
            yield return new TypeRegistration<AttachmentInput>("Microsoft.AttachmentInput");
            yield return new TypeRegistration<ConfirmInput>("Microsoft.ConfirmInput");
            yield return new TypeRegistration<NumberInput>("Microsoft.NumberInput");
            yield return new TypeRegistration<TextInput>("Microsoft.TextInput");
            yield return new TypeRegistration<ChoiceInput>("Microsoft.ChoiceInput");
            yield return new TypeRegistration<DateTimeInput>("Microsoft.DateTimeInput");
            yield return new TypeRegistration<OAuthInput>("Microsoft.OAuthInput");

            // Recognizers
            yield return new TypeRegistration<LuisRecognizer>("Microsoft.LuisRecognizer") { CustomDeserializer = new LuisRecognizerLoader(TypeFactory.Configuration) };
            yield return new TypeRegistration<RegexRecognizer>("Microsoft.RegexRecognizer");
            yield return new TypeRegistration<IntentPattern>("Microsoft.IntentPattern");
            yield return new TypeRegistration<MultiLanguageRecognizer>("Microsoft.MultiLanguageRecognizer");

            // Entity recognizers
            yield return new TypeRegistration<AgeEntityRecognizer>("Microsoft.AgeEntityRecognizer");
            yield return new TypeRegistration<ConfirmationEntityRecognizer>("Microsoft.ConfirmationEntityRecognizer");
            yield return new TypeRegistration<CurrencyEntityRecognizer>("Microsoft.CurrencyEntityRecognizer");
            yield return new TypeRegistration<DateTimeEntityRecognizer>("Microsoft.DateTimeEntityRecognizer");
            yield return new TypeRegistration<DimensionEntityRecognizer>("Microsoft.DimensionEntityRecognizer");
            yield return new TypeRegistration<EmailEntityRecognizer>("Microsoft.EmailEntityRecognizer");
            yield return new TypeRegistration<EntityRecognizer>("Microsoft.EntityRecognizer");
            yield return new TypeRegistration<EntityRecognizerSet>("Microsoft.EntityRecognizerSet");
            yield return new TypeRegistration<GuidEntityRecognizer>("Microsoft.GuidEntityRecognizer");
            yield return new TypeRegistration<HashtagEntityRecognizer>("Microsoft.HashtagEntityRecognizer");
            yield return new TypeRegistration<IpEntityRecognizer>("Microsoft.IpEntityRecognizer");
            yield return new TypeRegistration<MentionEntityRecognizer>("Microsoft.MentionEntityRecognizer");
            yield return new TypeRegistration<NumberEntityRecognizer>("Microsoft.NumberEntityRecognizer");
            yield return new TypeRegistration<NumberRangeEntityRecognizer>("Microsoft.NumberRangeEntityRecognizer");
            yield return new TypeRegistration<OrdinalEntityRecognizer>("Microsoft.OrdinalEntityRecognizer");
            yield return new TypeRegistration<PercentageEntityRecognizer>("Microsoft.PercentageEntityRecognizer");
            yield return new TypeRegistration<PhoneNumberEntityRecognizer>("Microsoft.PhoneNumberEntityRecognizer");
            yield return new TypeRegistration<TemperatureEntityRecognizer>("Microsoft.TemperatureEntityRecognizer");
            yield return new TypeRegistration<UrlEntityRecognizer>("Microsoft.UrlEntityRecognizer");

            // Dialogs
            yield return new TypeRegistration<AdaptiveDialog>("Microsoft.AdaptiveDialog");
        }

        public override IEnumerable<JsonConverter> GetConverters(Source.IRegistry registry, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new InterfaceConverter<OnCondition>(refResolver, registry, paths);
        }
    }
}
