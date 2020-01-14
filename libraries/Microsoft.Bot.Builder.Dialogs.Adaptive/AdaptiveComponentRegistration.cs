// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions;
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
            yield return new TypeRegistration<OnCondition>(OnCondition.DeclarativeType);
            yield return new TypeRegistration<OnError>(OnError.DeclarativeType);

            yield return new TypeRegistration<OnDialogEvent>(OnDialogEvent.DeclarativeType);
            yield return new TypeRegistration<OnCustomEvent>(OnCustomEvent.DeclarativeType);

            yield return new TypeRegistration<OnBeginDialog>(OnBeginDialog.DeclarativeType);
            yield return new TypeRegistration<OnCancelDialog>(OnCancelDialog.DeclarativeType);
            yield return new TypeRegistration<OnRepromptDialog>(OnRepromptDialog.DeclarativeType);

            yield return new TypeRegistration<OnIntent>(OnIntent.DeclarativeType);
            yield return new TypeRegistration<OnUnknownIntent>(OnUnknownIntent.DeclarativeType);

            yield return new TypeRegistration<OnActivity>(OnActivity.DeclarativeType);
            yield return new TypeRegistration<OnMessageActivity>(OnMessageActivity.DeclarativeType);
            yield return new TypeRegistration<OnMessageUpdateActivity>(OnMessageUpdateActivity.DeclarativeType);
            yield return new TypeRegistration<OnMessageDeleteActivity>(OnMessageDeleteActivity.DeclarativeType);
            yield return new TypeRegistration<OnMessageReactionActivity>(OnMessageReactionActivity.DeclarativeType);
            yield return new TypeRegistration<OnEventActivity>(OnEventActivity.DeclarativeType);
            yield return new TypeRegistration<OnInvokeActivity>(OnInvokeActivity.DeclarativeType);
            yield return new TypeRegistration<OnConversationUpdateActivity>(OnConversationUpdateActivity.DeclarativeType);
            yield return new TypeRegistration<OnEndOfConversationActivity>(OnEndOfConversationActivity.DeclarativeType);
            yield return new TypeRegistration<OnTypingActivity>(OnTypingActivity.DeclarativeType);
            yield return new TypeRegistration<OnHandoffActivity>(OnHandoffActivity.DeclarativeType);
            yield return new TypeRegistration<OnChooseIntent>(OnChooseIntent.DeclarativeType);

            yield return new TypeRegistration<OnEndOfActions>(OnEndOfActions.DeclarativeType);
            yield return new TypeRegistration<OnChooseProperty>(OnChooseProperty.DeclarativeType);
            yield return new TypeRegistration<OnChooseEntity>(OnChooseEntity.DeclarativeType);
            yield return new TypeRegistration<OnClearProperty>(OnClearProperty.DeclarativeType);
            yield return new TypeRegistration<OnAssignEntity>(OnAssignEntity.DeclarativeType);

            // Actions
            yield return new TypeRegistration<BeginDialog>(BeginDialog.DeclarativeType);
            yield return new TypeRegistration<CancelAllDialogs>(CancelAllDialogs.DeclarativeType);
            yield return new TypeRegistration<DebugBreak>(DebugBreak.DeclarativeType);
            yield return new TypeRegistration<DeleteProperty>(DeleteProperty.DeclarativeType);
            yield return new TypeRegistration<DeleteProperties>(DeleteProperties.DeclarativeType);
            yield return new TypeRegistration<EditArray>(EditArray.DeclarativeType);
            yield return new TypeRegistration<EditActions>(EditActions.DeclarativeType);
            yield return new TypeRegistration<EmitEvent>(EmitEvent.DeclarativeType);
            yield return new TypeRegistration<EndDialog>(EndDialog.DeclarativeType);
            yield return new TypeRegistration<EndTurn>(EndTurn.DeclarativeType);
            yield return new TypeRegistration<Foreach>(Foreach.DeclarativeType);
            yield return new TypeRegistration<ForeachPage>(ForeachPage.DeclarativeType);
            yield return new TypeRegistration<HttpRequest>(HttpRequest.DeclarativeType);
            yield return new TypeRegistration<IfCondition>(IfCondition.DeclarativeType);
            yield return new TypeRegistration<InitProperty>(InitProperty.DeclarativeType);
            yield return new TypeRegistration<LogAction>(LogAction.DeclarativeType);
            yield return new TypeRegistration<RepeatDialog>(RepeatDialog.DeclarativeType);
            yield return new TypeRegistration<ReplaceDialog>(ReplaceDialog.DeclarativeType);
            yield return new TypeRegistration<SendActivity>(SendActivity.DeclarativeType);
            yield return new TypeRegistration<SetProperty>(SetProperty.DeclarativeType);
            yield return new TypeRegistration<SetProperties>(SetProperties.DeclarativeType);
            yield return new TypeRegistration<SwitchCondition>(SwitchCondition.DeclarativeType);
            yield return new TypeRegistration<TraceActivity>(TraceActivity.DeclarativeType);
            yield return new TypeRegistration<GotoAction>(GotoAction.DeclarativeType);
            yield return new TypeRegistration<BreakLoop>(BreakLoop.DeclarativeType);
            yield return new TypeRegistration<ContinueLoop>(ContinueLoop.DeclarativeType);
            yield return new TypeRegistration<UpdateActivity>(UpdateActivity.DeclarativeType);
            yield return new TypeRegistration<DeleteActivity>(DeleteActivity.DeclarativeType);
            yield return new TypeRegistration<GetActivityMembers>(GetActivityMembers.DeclarativeType);
            yield return new TypeRegistration<GetConversationMembers>(GetConversationMembers.DeclarativeType);
            yield return new TypeRegistration<SignOutUser>(SignOutUser.DeclarativeType);

            // Inputs
            yield return new TypeRegistration<AttachmentInput>(AttachmentInput.DeclarativeType);
            yield return new TypeRegistration<ConfirmInput>(ConfirmInput.DeclarativeType);
            yield return new TypeRegistration<NumberInput>(NumberInput.DeclarativeType);
            yield return new TypeRegistration<TextInput>(TextInput.DeclarativeType);
            yield return new TypeRegistration<ChoiceInput>(ChoiceInput.DeclarativeType);
            yield return new TypeRegistration<DateTimeInput>(DateTimeInput.DeclarativeType);
            yield return new TypeRegistration<OAuthInput>(OAuthInput.DeclarativeType);
            yield return new TypeRegistration<Ask>(Ask.DeclarativeType);

            // Recognizers
            yield return new TypeRegistration<LuisRecognizer>(LuisRecognizer.DeclarativeType) { CustomDeserializer = new LuisRecognizerLoader(TypeFactory.Configuration) };
            yield return new TypeRegistration<RegexRecognizer>(RegexRecognizer.DeclarativeType);
            yield return new TypeRegistration<MultiLanguageRecognizer>(MultiLanguageRecognizer.DeclarativeType);
            yield return new TypeRegistration<RecognizerSet>(RecognizerSet.DeclarativeType);
            yield return new TypeRegistration<CrossTrainedRecognizerSet>(CrossTrainedRecognizerSet.DeclarativeType);
            yield return new TypeRegistration<ValueRecognizer>(ValueRecognizer.DeclarativeType);

            // Entity recognizers
            yield return new TypeRegistration<AgeEntityRecognizer>(AgeEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<ConfirmationEntityRecognizer>(ConfirmationEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<CurrencyEntityRecognizer>(CurrencyEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<DateTimeEntityRecognizer>(DateTimeEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<DimensionEntityRecognizer>(DimensionEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<EmailEntityRecognizer>(EmailEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<EntityRecognizerSet>(EntityRecognizerSet.DeclarativeType);
            yield return new TypeRegistration<GuidEntityRecognizer>(GuidEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<HashtagEntityRecognizer>(HashtagEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<IpEntityRecognizer>(IpEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<MentionEntityRecognizer>(MentionEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<NumberEntityRecognizer>(NumberEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<NumberRangeEntityRecognizer>(NumberRangeEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<OrdinalEntityRecognizer>(OrdinalEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<PercentageEntityRecognizer>(PercentageEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<PhoneNumberEntityRecognizer>(PhoneNumberEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<RegexEntityRecognizer>(RegexEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<TemperatureEntityRecognizer>(TemperatureEntityRecognizer.DeclarativeType);
            yield return new TypeRegistration<UrlEntityRecognizer>(UrlEntityRecognizer.DeclarativeType);

            // selectors
            yield return new TypeRegistration<ConditionalSelector>(ConditionalSelector.DeclarativeType);
            yield return new TypeRegistration<FirstSelector>(FirstSelector.DeclarativeType);
            yield return new TypeRegistration<MostSpecificSelector>(MostSpecificSelector.DeclarativeType);
            yield return new TypeRegistration<RandomSelector>(RandomSelector.DeclarativeType);
            yield return new TypeRegistration<TrueSelector>(TrueSelector.DeclarativeType);

            // Generators
            yield return new TypeRegistration<ResourceMultiLanguageGenerator>(ResourceMultiLanguageGenerator.DeclarativeType);
            yield return new TypeRegistration<MultiLanguageGenerator>(MultiLanguageGenerator.DeclarativeType);
            yield return new TypeRegistration<TemplateEngineLanguageGenerator>(TemplateEngineLanguageGenerator.DeclarativeType);

            // Dialogs
            yield return new TypeRegistration<AdaptiveDialog>(AdaptiveDialog.DeclarativeType);

            // test actions
            yield return new TypeRegistration<AssertCondition>(AssertCondition.DeclarativeType);
            yield return new TypeRegistration<TestScript>(TestScript.DeclarativeType);
            yield return new TypeRegistration<UserSays>(UserSays.DeclarativeType);
            yield return new TypeRegistration<UserTyping>(UserTyping.DeclarativeType);
            yield return new TypeRegistration<UserConversationUpdate>(UserConversationUpdate.DeclarativeType);
            yield return new TypeRegistration<UserActivity>(UserActivity.DeclarativeType);
            yield return new TypeRegistration<UserDelay>(UserDelay.DeclarativeType);
            yield return new TypeRegistration<AssertReply>(AssertReply.DeclarativeType);
            yield return new TypeRegistration<AssertReplyOneOf>(AssertReplyOneOf.DeclarativeType);
            yield return new TypeRegistration<AssertReplyActivity>(AssertReplyActivity.DeclarativeType);
        }

        public override IEnumerable<JsonConverter> GetConverters(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new InterfaceConverter<OnCondition>(refResolver, sourceMap, paths);
            yield return new InterfaceConverter<TestAction>(refResolver, sourceMap, paths);
            yield return new InterfaceConverter<EntityRecognizer>(refResolver, sourceMap, paths);
            yield return new InterfaceConverter<ITriggerSelector>(refResolver, sourceMap, paths);
            yield return new ExpressionPropertyConverter<ChoiceSet>();
            yield return new ExpressionPropertyConverter<ExpressionProperty<List<string>>>();
            yield return new ActivityTemplateConverter();
            yield return new JObjectConverter(refResolver);
        }
    }
}
