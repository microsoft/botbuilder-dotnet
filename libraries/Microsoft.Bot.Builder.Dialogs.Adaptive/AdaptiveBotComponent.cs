// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdaptiveExpressions;
using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Functions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Functions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// <see cref="BotComponent"/> for adaptive components.
    /// </summary>
    public class AdaptiveBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            // Adaptive dialog functions.
            Expression.Functions.Add(IsDialogActiveFunction.Name, new IsDialogActiveFunction());
            Expression.Functions.Add(IsDialogActiveFunction.Alias, new IsDialogActiveFunction());
            Expression.Functions.Add(HasPendingActionsFunction.Name, new HasPendingActionsFunction());

            // Declarative types.

            // Conditionals
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnCondition>(OnCondition.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnError>(OnError.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnDialogEvent>(OnDialogEvent.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnBeginDialog>(OnBeginDialog.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnCancelDialog>(OnCancelDialog.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnRepromptDialog>(OnRepromptDialog.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnIntent>(OnIntent.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnUnknownIntent>(OnUnknownIntent.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnActivity>(OnActivity.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnMessageActivity>(OnMessageActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnMessageUpdateActivity>(OnMessageUpdateActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnMessageDeleteActivity>(OnMessageDeleteActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnMessageReactionActivity>(OnMessageReactionActivity.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnEventActivity>(OnEventActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnInvokeActivity>(OnInvokeActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnConversationUpdateActivity>(OnConversationUpdateActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnEndOfConversationActivity>(OnEndOfConversationActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnTypingActivity>(OnTypingActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnInstallationUpdateActivity>(OnInstallationUpdateActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnHandoffActivity>(OnHandoffActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnChooseIntent>(OnChooseIntent.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnQnAMatch>(OnQnAMatch.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnEndOfActions>(OnEndOfActions.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnChooseProperty>(OnChooseProperty.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnChooseEntity>(OnChooseEntity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnAssignEntity>(OnAssignEntity.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnCommandActivity>(OnCommandActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnCommandResultActivity>(OnCommandResultActivity.Kind));

            // Actions
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<BeginDialog>(BeginDialog.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<BeginSkill>(BeginSkill.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<CancelDialog>(CancelDialog.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<CancelAllDialogs>(CancelAllDialogs.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<DebugBreak>(DebugBreak.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<DeleteProperty>(DeleteProperty.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<DeleteProperties>(DeleteProperties.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<EditArray>(EditArray.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<EditActions>(EditActions.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<EmitEvent>(EmitEvent.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<EndDialog>(EndDialog.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<EndTurn>(EndTurn.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<Foreach>(Foreach.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ForeachPage>(ForeachPage.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<HttpRequest>(HttpRequest.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<IfCondition>(IfCondition.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<LogAction>(LogAction.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<RepeatDialog>(RepeatDialog.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ReplaceDialog>(ReplaceDialog.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SendActivity>(SendActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SetProperty>(SetProperty.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SetProperties>(SetProperties.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SwitchCondition>(SwitchCondition.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<TraceActivity>(TraceActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ThrowException>(ThrowException.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<GotoAction>(GotoAction.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<BreakLoop>(BreakLoop.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ContinueLoop>(ContinueLoop.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UpdateActivity>(UpdateActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<DeleteActivity>(DeleteActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<GetActivityMembers>(GetActivityMembers.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<GetConversationMembers>(GetConversationMembers.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<GetConversationReference>(GetConversationReference.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SignOutUser>(SignOutUser.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<TelemetryTrackEventAction>(TelemetryTrackEventAction.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ContinueConversation>(ContinueConversation.Kind));

            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ContinueConversationLater>(ContinueConversationLater.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SendHandoffActivity>(SendHandoffActivity.Kind));

            // Inputs
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AttachmentInput>(AttachmentInput.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ConfirmInput>(ConfirmInput.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<NumberInput>(NumberInput.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<TextInput>(TextInput.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ChoiceInput>(ChoiceInput.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<DateTimeInput>(DateTimeInput.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OAuthInput>(OAuthInput.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<Ask>(Ask.Kind));

            // Recognizers
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<RegexRecognizer>(RegexRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<MultiLanguageRecognizer>(MultiLanguageRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<RecognizerSet>(RecognizerSet.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<CrossTrainedRecognizerSet>(CrossTrainedRecognizerSet.Kind));

            // Entity recognizers
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AgeEntityRecognizer>(AgeEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ConfirmationEntityRecognizer>(ConfirmationEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<CurrencyEntityRecognizer>(CurrencyEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<DateTimeEntityRecognizer>(DateTimeEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<DimensionEntityRecognizer>(DimensionEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<EmailEntityRecognizer>(EmailEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<EntityRecognizerSet>(EntityRecognizerSet.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<GuidEntityRecognizer>(GuidEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<HashtagEntityRecognizer>(HashtagEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<IpEntityRecognizer>(IpEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<MentionEntityRecognizer>(MentionEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<NumberEntityRecognizer>(NumberEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<NumberRangeEntityRecognizer>(NumberRangeEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OrdinalEntityRecognizer>(OrdinalEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<PercentageEntityRecognizer>(PercentageEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<PhoneNumberEntityRecognizer>(PhoneNumberEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<RegexEntityRecognizer>(RegexEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<TemperatureEntityRecognizer>(TemperatureEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UrlEntityRecognizer>(UrlEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ChannelMentionEntityRecognizer>(ChannelMentionEntityRecognizer.Kind));

            // selectors
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ConditionalSelector>(ConditionalSelector.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<FirstSelector>(FirstSelector.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<MostSpecificSelector>(MostSpecificSelector.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<RandomSelector>(RandomSelector.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<TrueSelector>(TrueSelector.Kind));

            // Generators
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ResourceMultiLanguageGenerator>(ResourceMultiLanguageGenerator.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<MultiLanguageGenerator>(MultiLanguageGenerator.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<TemplateEngineLanguageGenerator>(TemplateEngineLanguageGenerator.Kind));

            // Dialogs
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AdaptiveDialog>(AdaptiveDialog.Kind));

            // Declarative converters.
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<OnCondition>>();
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<EntityRecognizer>>();
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<TriggerSelector>>();

            services.AddSingleton<JsonConverterFactory>(
                sp => new LambdaJsonConverterFactory((r, s) => new ITemplateActivityConverter(r, s)));

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<IntExpressionConverter>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<NumberExpressionConverter>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<StringExpressionConverter>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ValueExpressionConverter>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<BoolExpressionConverter>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<IntExpressionConverter>>();

            services.AddSingleton<JsonConverterFactory>(
                sp => new LambdaJsonConverterFactory((r, s) => new DialogExpressionConverter(r, s)));

            services.AddSingleton<JsonConverterFactory>(
                sp => new LambdaJsonConverterFactory((r, s) => new DialogSetConverter(r)));

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<ChoiceSet>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<ChoiceFactoryOptions>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<FindChoicesOptions>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<ConversationReference>>>();

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ArrayExpressionConverter<string>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ArrayExpressionConverter<Choice>>>();

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<EnumExpressionConverter<ActionChangeType>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<EnumExpressionConverter<EditArray.ArrayChangeType>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<EnumExpressionConverter<AttachmentOutputFormat>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<EnumExpressionConverter<ListStyle>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<EnumExpressionConverter<ChoiceOutputFormat>>>();

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ChoiceSetConverter>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ActivityTemplateConverter>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<StaticActivityTemplateConverter>>();

            services.AddSingleton<JsonConverterFactory>(
                sp => new LambdaJsonConverterFactory((r, s) => new JObjectConverter(r, s)));

            // Unfortunately the code below is not DI friendly in that it needs to iterate a resource explorer, so
            // we build a service provider to get the resource explorer and iterate schemas.
            using (var serviceScope = services.BuildServiceProvider().CreateScope())
            {
                var resourceExplorer = serviceScope.ServiceProvider.GetService<ResourceExplorer>();

                if (resourceExplorer != null)
                {
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
                                services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<DynamicBeginDialog>(resourceId) { CustomDeserializer = new DynamicBeginDialogDeserializer(sp.GetRequiredService<ResourceExplorer>(), resourceId) });
                                break;
                        }
                    }
                }
            }
        }
    }
}
