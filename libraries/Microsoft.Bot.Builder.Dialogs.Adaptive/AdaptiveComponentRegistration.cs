// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Skills;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class AdaptiveComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public virtual IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            // Conditionals
            yield return new DeclarativeType<OnCondition>(OnCondition.Kind);
            yield return new DeclarativeType<OnError>(OnError.Kind);

            yield return new DeclarativeType<OnDialogEvent>(OnDialogEvent.Kind);
            yield return new DeclarativeType<OnCustomEvent>(OnCustomEvent.Kind);

            yield return new DeclarativeType<OnBeginDialog>(OnBeginDialog.Kind);
            yield return new DeclarativeType<OnCancelDialog>(OnCancelDialog.Kind);
            yield return new DeclarativeType<OnRepromptDialog>(OnRepromptDialog.Kind);

            yield return new DeclarativeType<OnIntent>(OnIntent.Kind);
            yield return new DeclarativeType<OnUnknownIntent>(OnUnknownIntent.Kind);

            yield return new DeclarativeType<OnActivity>(OnActivity.Kind);
            yield return new DeclarativeType<OnMessageActivity>(OnMessageActivity.Kind);
            yield return new DeclarativeType<OnMessageUpdateActivity>(OnMessageUpdateActivity.Kind);
            yield return new DeclarativeType<OnMessageDeleteActivity>(OnMessageDeleteActivity.Kind);
            yield return new DeclarativeType<OnMessageReactionActivity>(OnMessageReactionActivity.Kind);
            yield return new DeclarativeType<OnEventActivity>(OnEventActivity.Kind);
            yield return new DeclarativeType<OnInvokeActivity>(OnInvokeActivity.Kind);
            yield return new DeclarativeType<OnConversationUpdateActivity>(OnConversationUpdateActivity.Kind);
            yield return new DeclarativeType<OnEndOfConversationActivity>(OnEndOfConversationActivity.Kind);
            yield return new DeclarativeType<OnTypingActivity>(OnTypingActivity.Kind);
            yield return new DeclarativeType<OnHandoffActivity>(OnHandoffActivity.Kind);
            yield return new DeclarativeType<OnChooseIntent>(OnChooseIntent.Kind);

            yield return new DeclarativeType<OnEndOfActions>(OnEndOfActions.Kind);
            yield return new DeclarativeType<OnChooseProperty>(OnChooseProperty.Kind);
            yield return new DeclarativeType<OnChooseEntity>(OnChooseEntity.Kind);
            yield return new DeclarativeType<OnAssignEntity>(OnAssignEntity.Kind);

            // Actions
            yield return new DeclarativeType<BeginDialog>(BeginDialog.Kind);
            yield return new DeclarativeType<CancelAllDialogs>(CancelAllDialogs.Kind);
            yield return new DeclarativeType<DebugBreak>(DebugBreak.Kind);
            yield return new DeclarativeType<DeleteProperty>(DeleteProperty.Kind);
            yield return new DeclarativeType<DeleteProperties>(DeleteProperties.Kind);
            yield return new DeclarativeType<EditArray>(EditArray.Kind);
            yield return new DeclarativeType<EditActions>(EditActions.Kind);
            yield return new DeclarativeType<EmitEvent>(EmitEvent.Kind);
            yield return new DeclarativeType<EndDialog>(EndDialog.Kind);
            yield return new DeclarativeType<EndTurn>(EndTurn.Kind);
            yield return new DeclarativeType<Foreach>(Foreach.Kind);
            yield return new DeclarativeType<ForeachPage>(ForeachPage.Kind);
            yield return new DeclarativeType<HttpRequest>(HttpRequest.Kind);
            yield return new DeclarativeType<IfCondition>(IfCondition.Kind);
            yield return new DeclarativeType<LogAction>(LogAction.Kind);
            yield return new DeclarativeType<RepeatDialog>(RepeatDialog.Kind);
            yield return new DeclarativeType<ReplaceDialog>(ReplaceDialog.Kind);
            yield return new DeclarativeType<SendActivity>(SendActivity.Kind);
            yield return new DeclarativeType<SetProperty>(SetProperty.Kind);
            yield return new DeclarativeType<SetProperties>(SetProperties.Kind);
            yield return new DeclarativeType<SwitchCondition>(SwitchCondition.Kind);
            yield return new DeclarativeType<TraceActivity>(TraceActivity.Kind);
            yield return new DeclarativeType<GotoAction>(GotoAction.Kind);
            yield return new DeclarativeType<BreakLoop>(BreakLoop.Kind);
            yield return new DeclarativeType<ContinueLoop>(ContinueLoop.Kind);
            yield return new DeclarativeType<UpdateActivity>(UpdateActivity.Kind);
            yield return new DeclarativeType<DeleteActivity>(DeleteActivity.Kind);
            yield return new DeclarativeType<GetActivityMembers>(GetActivityMembers.Kind);
            yield return new DeclarativeType<GetConversationMembers>(GetConversationMembers.Kind);
            yield return new DeclarativeType<SignOutUser>(SignOutUser.Kind);

            // Inputs
            yield return new DeclarativeType<AttachmentInput>(AttachmentInput.Kind);
            yield return new DeclarativeType<ConfirmInput>(ConfirmInput.Kind);
            yield return new DeclarativeType<NumberInput>(NumberInput.Kind);
            yield return new DeclarativeType<TextInput>(TextInput.Kind);
            yield return new DeclarativeType<ChoiceInput>(ChoiceInput.Kind);
            yield return new DeclarativeType<DateTimeInput>(DateTimeInput.Kind);
            yield return new DeclarativeType<OAuthInput>(OAuthInput.Kind);
            yield return new DeclarativeType<Ask>(Ask.Kind);

            // Recognizers
            yield return new DeclarativeType<RegexRecognizer>(RegexRecognizer.Kind);
            yield return new DeclarativeType<MultiLanguageRecognizer>(MultiLanguageRecognizer.Kind);
            yield return new DeclarativeType<RecognizerSet>(RecognizerSet.Kind);
            yield return new DeclarativeType<CrossTrainedRecognizerSet>(CrossTrainedRecognizerSet.Kind);

            // Entity recognizers
            yield return new DeclarativeType<AgeEntityRecognizer>(AgeEntityRecognizer.Kind);
            yield return new DeclarativeType<ConfirmationEntityRecognizer>(ConfirmationEntityRecognizer.Kind);
            yield return new DeclarativeType<CurrencyEntityRecognizer>(CurrencyEntityRecognizer.Kind);
            yield return new DeclarativeType<DateTimeEntityRecognizer>(DateTimeEntityRecognizer.Kind);
            yield return new DeclarativeType<DimensionEntityRecognizer>(DimensionEntityRecognizer.Kind);
            yield return new DeclarativeType<EmailEntityRecognizer>(EmailEntityRecognizer.Kind);
            yield return new DeclarativeType<EntityRecognizerSet>(EntityRecognizerSet.Kind);
            yield return new DeclarativeType<GuidEntityRecognizer>(GuidEntityRecognizer.Kind);
            yield return new DeclarativeType<HashtagEntityRecognizer>(HashtagEntityRecognizer.Kind);
            yield return new DeclarativeType<IpEntityRecognizer>(IpEntityRecognizer.Kind);
            yield return new DeclarativeType<MentionEntityRecognizer>(MentionEntityRecognizer.Kind);
            yield return new DeclarativeType<NumberEntityRecognizer>(NumberEntityRecognizer.Kind);
            yield return new DeclarativeType<NumberRangeEntityRecognizer>(NumberRangeEntityRecognizer.Kind);
            yield return new DeclarativeType<OrdinalEntityRecognizer>(OrdinalEntityRecognizer.Kind);
            yield return new DeclarativeType<PercentageEntityRecognizer>(PercentageEntityRecognizer.Kind);
            yield return new DeclarativeType<PhoneNumberEntityRecognizer>(PhoneNumberEntityRecognizer.Kind);
            yield return new DeclarativeType<RegexEntityRecognizer>(RegexEntityRecognizer.Kind);
            yield return new DeclarativeType<TemperatureEntityRecognizer>(TemperatureEntityRecognizer.Kind);
            yield return new DeclarativeType<UrlEntityRecognizer>(UrlEntityRecognizer.Kind);

            // selectors
            yield return new DeclarativeType<ConditionalSelector>(ConditionalSelector.Kind);
            yield return new DeclarativeType<FirstSelector>(FirstSelector.Kind);
            yield return new DeclarativeType<MostSpecificSelector>(MostSpecificSelector.Kind);
            yield return new DeclarativeType<RandomSelector>(RandomSelector.Kind);
            yield return new DeclarativeType<TrueSelector>(TrueSelector.Kind);

            // Generators
            yield return new DeclarativeType<ResourceMultiLanguageGenerator>(ResourceMultiLanguageGenerator.Kind);
            yield return new DeclarativeType<MultiLanguageGenerator>(MultiLanguageGenerator.Kind);
            yield return new DeclarativeType<TemplateEngineLanguageGenerator>(TemplateEngineLanguageGenerator.Kind);

            // Dialogs
            yield return new DeclarativeType<AdaptiveDialog>(AdaptiveDialog.Kind);
            yield return new DeclarativeType<AdaptiveSkillDialog>(AdaptiveSkillDialog.Kind);

            // register x.dialog.schema/x.dialog as DynamicBeginDialog $kind="x" => DynamicBeginDialog(x.dialog) resource.
            foreach (var schema in resourceExplorer.GetResources(".schema").Where(s => resourceExplorer.GetTypeForKind(Path.GetFileNameWithoutExtension(s.Id)) == null))
            {
                // x.dialog.schema => resourceType=dialog resourceId=x.dialog $kind=x
                var resourceId = Path.GetFileNameWithoutExtension(schema.Id);
                var resourceType = Path.GetExtension(resourceId).TrimStart('.').ToLowerInvariant();

                // load dynamic dialogs
                switch (resourceType)
                {
                    case "dialog":
                        // register foo.dialog as $kind
                        yield return new DeclarativeType<DynamicBeginDialog>(resourceId) { CustomDeserializer = new DynamicBeginDialogDeserializer(resourceExplorer, resourceId) };
                        break;
                }
            }
        }

        public virtual IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new InterfaceConverter<OnCondition>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<EntityRecognizer>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<TriggerSelector>(resourceExplorer, sourceContext);

            yield return new IntExpressionConverter();
            yield return new NumberExpressionConverter();
            yield return new StringExpressionConverter();
            yield return new ValueExpressionConverter();
            yield return new BoolExpressionConverter();
            yield return new DialogExpressionConverter(resourceExplorer, sourceContext);

            yield return new ObjectExpressionConverter<ChoiceSet>();
            yield return new ObjectExpressionConverter<ChoiceFactoryOptions>();
            yield return new ObjectExpressionConverter<FindChoicesOptions>();

            yield return new ArrayExpressionConverter<string>();
            yield return new ArrayExpressionConverter<Choice>();

            yield return new EnumExpressionConverter<ActionChangeType>();
            yield return new EnumExpressionConverter<EditArray.ArrayChangeType>();
            yield return new EnumExpressionConverter<AttachmentOutputFormat>();
            yield return new EnumExpressionConverter<ListStyle>();
            yield return new EnumExpressionConverter<ChoiceOutputFormat>();

            yield return new ChoiceSetConverter();
            yield return new ActivityTemplateConverter();
            yield return new JObjectConverter(resourceExplorer, sourceContext);
        }
    }
}
