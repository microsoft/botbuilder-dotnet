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
            yield return new DeclarativeType<OnCondition>(OnCondition.DeclarativeType);
            yield return new DeclarativeType<OnError>(OnError.DeclarativeType);

            yield return new DeclarativeType<OnDialogEvent>(OnDialogEvent.DeclarativeType);
            yield return new DeclarativeType<OnCustomEvent>(OnCustomEvent.DeclarativeType);

            yield return new DeclarativeType<OnBeginDialog>(OnBeginDialog.DeclarativeType);
            yield return new DeclarativeType<OnCancelDialog>(OnCancelDialog.DeclarativeType);
            yield return new DeclarativeType<OnRepromptDialog>(OnRepromptDialog.DeclarativeType);

            yield return new DeclarativeType<OnIntent>(OnIntent.DeclarativeType);
            yield return new DeclarativeType<OnUnknownIntent>(OnUnknownIntent.DeclarativeType);

            yield return new DeclarativeType<OnActivity>(OnActivity.DeclarativeType);
            yield return new DeclarativeType<OnMessageActivity>(OnMessageActivity.DeclarativeType);
            yield return new DeclarativeType<OnMessageUpdateActivity>(OnMessageUpdateActivity.DeclarativeType);
            yield return new DeclarativeType<OnMessageDeleteActivity>(OnMessageDeleteActivity.DeclarativeType);
            yield return new DeclarativeType<OnMessageReactionActivity>(OnMessageReactionActivity.DeclarativeType);
            yield return new DeclarativeType<OnEventActivity>(OnEventActivity.DeclarativeType);
            yield return new DeclarativeType<OnInvokeActivity>(OnInvokeActivity.DeclarativeType);
            yield return new DeclarativeType<OnConversationUpdateActivity>(OnConversationUpdateActivity.DeclarativeType);
            yield return new DeclarativeType<OnEndOfConversationActivity>(OnEndOfConversationActivity.DeclarativeType);
            yield return new DeclarativeType<OnTypingActivity>(OnTypingActivity.DeclarativeType);
            yield return new DeclarativeType<OnHandoffActivity>(OnHandoffActivity.DeclarativeType);
            yield return new DeclarativeType<OnChooseIntent>(OnChooseIntent.DeclarativeType);

            yield return new DeclarativeType<OnEndOfActions>(OnEndOfActions.DeclarativeType);
            yield return new DeclarativeType<OnChooseProperty>(OnChooseProperty.DeclarativeType);
            yield return new DeclarativeType<OnChooseEntity>(OnChooseEntity.DeclarativeType);
            yield return new DeclarativeType<OnAssignEntity>(OnAssignEntity.DeclarativeType);

            // Actions
            yield return new DeclarativeType<BeginDialog>(BeginDialog.DeclarativeType);
            yield return new DeclarativeType<CancelAllDialogs>(CancelAllDialogs.DeclarativeType);
            yield return new DeclarativeType<DebugBreak>(DebugBreak.DeclarativeType);
            yield return new DeclarativeType<DeleteProperty>(DeleteProperty.DeclarativeType);
            yield return new DeclarativeType<DeleteProperties>(DeleteProperties.DeclarativeType);
            yield return new DeclarativeType<EditArray>(EditArray.DeclarativeType);
            yield return new DeclarativeType<EditActions>(EditActions.DeclarativeType);
            yield return new DeclarativeType<EmitEvent>(EmitEvent.DeclarativeType);
            yield return new DeclarativeType<EndDialog>(EndDialog.DeclarativeType);
            yield return new DeclarativeType<EndTurn>(EndTurn.DeclarativeType);
            yield return new DeclarativeType<Foreach>(Foreach.DeclarativeType);
            yield return new DeclarativeType<ForeachPage>(ForeachPage.DeclarativeType);
            yield return new DeclarativeType<HttpRequest>(HttpRequest.DeclarativeType);
            yield return new DeclarativeType<IfCondition>(IfCondition.DeclarativeType);
            yield return new DeclarativeType<LogAction>(LogAction.DeclarativeType);
            yield return new DeclarativeType<RepeatDialog>(RepeatDialog.DeclarativeType);
            yield return new DeclarativeType<ReplaceDialog>(ReplaceDialog.DeclarativeType);
            yield return new DeclarativeType<SendActivity>(SendActivity.DeclarativeType);
            yield return new DeclarativeType<SetProperty>(SetProperty.DeclarativeType);
            yield return new DeclarativeType<SetProperties>(SetProperties.DeclarativeType);
            yield return new DeclarativeType<SwitchCondition>(SwitchCondition.DeclarativeType);
            yield return new DeclarativeType<TraceActivity>(TraceActivity.DeclarativeType);
            yield return new DeclarativeType<GotoAction>(GotoAction.DeclarativeType);
            yield return new DeclarativeType<BreakLoop>(BreakLoop.DeclarativeType);
            yield return new DeclarativeType<ContinueLoop>(ContinueLoop.DeclarativeType);
            yield return new DeclarativeType<UpdateActivity>(UpdateActivity.DeclarativeType);
            yield return new DeclarativeType<DeleteActivity>(DeleteActivity.DeclarativeType);
            yield return new DeclarativeType<GetActivityMembers>(GetActivityMembers.DeclarativeType);
            yield return new DeclarativeType<GetConversationMembers>(GetConversationMembers.DeclarativeType);
            yield return new DeclarativeType<SignOutUser>(SignOutUser.DeclarativeType);

            // Inputs
            yield return new DeclarativeType<AttachmentInput>(AttachmentInput.DeclarativeType);
            yield return new DeclarativeType<ConfirmInput>(ConfirmInput.DeclarativeType);
            yield return new DeclarativeType<NumberInput>(NumberInput.DeclarativeType);
            yield return new DeclarativeType<TextInput>(TextInput.DeclarativeType);
            yield return new DeclarativeType<ChoiceInput>(ChoiceInput.DeclarativeType);
            yield return new DeclarativeType<DateTimeInput>(DateTimeInput.DeclarativeType);
            yield return new DeclarativeType<OAuthInput>(OAuthInput.DeclarativeType);
            yield return new DeclarativeType<Ask>(Ask.DeclarativeType);

            // Recognizers
            yield return new DeclarativeType<RegexRecognizer>(RegexRecognizer.DeclarativeType);
            yield return new DeclarativeType<MultiLanguageRecognizer>(MultiLanguageRecognizer.DeclarativeType);
            yield return new DeclarativeType<RecognizerSet>(RecognizerSet.DeclarativeType);
            yield return new DeclarativeType<CrossTrainedRecognizerSet>(CrossTrainedRecognizerSet.DeclarativeType);
            yield return new DeclarativeType<ValueRecognizer>(ValueRecognizer.DeclarativeType);

            // Entity recognizers
            yield return new DeclarativeType<AgeEntityRecognizer>(AgeEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<ConfirmationEntityRecognizer>(ConfirmationEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<CurrencyEntityRecognizer>(CurrencyEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<DateTimeEntityRecognizer>(DateTimeEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<DimensionEntityRecognizer>(DimensionEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<EmailEntityRecognizer>(EmailEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<EntityRecognizerSet>(EntityRecognizerSet.DeclarativeType);
            yield return new DeclarativeType<GuidEntityRecognizer>(GuidEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<HashtagEntityRecognizer>(HashtagEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<IpEntityRecognizer>(IpEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<MentionEntityRecognizer>(MentionEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<NumberEntityRecognizer>(NumberEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<NumberRangeEntityRecognizer>(NumberRangeEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<OrdinalEntityRecognizer>(OrdinalEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<PercentageEntityRecognizer>(PercentageEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<PhoneNumberEntityRecognizer>(PhoneNumberEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<RegexEntityRecognizer>(RegexEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<TemperatureEntityRecognizer>(TemperatureEntityRecognizer.DeclarativeType);
            yield return new DeclarativeType<UrlEntityRecognizer>(UrlEntityRecognizer.DeclarativeType);

            // selectors
            yield return new DeclarativeType<ConditionalSelector>(ConditionalSelector.DeclarativeType);
            yield return new DeclarativeType<FirstSelector>(FirstSelector.DeclarativeType);
            yield return new DeclarativeType<MostSpecificSelector>(MostSpecificSelector.DeclarativeType);
            yield return new DeclarativeType<RandomSelector>(RandomSelector.DeclarativeType);
            yield return new DeclarativeType<TrueSelector>(TrueSelector.DeclarativeType);

            // Generators
            yield return new DeclarativeType<ResourceMultiLanguageGenerator>(ResourceMultiLanguageGenerator.DeclarativeType);
            yield return new DeclarativeType<MultiLanguageGenerator>(MultiLanguageGenerator.DeclarativeType);
            yield return new DeclarativeType<TemplateEngineLanguageGenerator>(TemplateEngineLanguageGenerator.DeclarativeType);

            // Dialogs
            yield return new DeclarativeType<AdaptiveDialog>(AdaptiveDialog.DeclarativeType);
            yield return new DeclarativeType<AdaptiveSkillDialog>(AdaptiveSkillDialog.DeclarativeType);

            // register x.dialog.schema/x.dialog as DynamicBeginDialog $kind="x" => DynamicBeginDialog(x.dialog) resource.
            foreach (var schema in resourceExplorer.GetResources(".schema").Where(s => resourceExplorer.GetTypeForKind(Path.GetFileNameWithoutExtension(s.Id)) == null))
            {
                // x.dialog.schema => resourceType=dialog resourceId=x.dialog $kind=x
                var resourceId = Path.GetFileNameWithoutExtension(schema.Id);
                var resourceType = Path.GetExtension(resourceId).TrimStart('.').ToLowerInvariant();
                var kind = Path.GetFileNameWithoutExtension(resourceId);

                // load dynamic dialogs
                switch (resourceType)
                {
                    case "dialog":
                        yield return new DeclarativeType<DynamicBeginDialog>(kind) { CustomDeserializer = new DynamicBeginDialogDeserializer(resourceExplorer, resourceId) };
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
