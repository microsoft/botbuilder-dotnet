// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
// using Microsoft.Recognizers.Text;

namespace AspNetCore_LUIS_Bot
{
    public class LuisBot : IBot
    {
        private const double LUIS_INTENT_THRESHOLD = 0.2d;

        private readonly DialogSet _dialogs;
        private readonly BotFrameworkOptions _options;
        private readonly LuisBotStateAccessors _stateAccessors;

        public LuisBot(IOptions<BotFrameworkOptions> options, LuisBotStateAccessors accessors)
        {
            _options = options.Value;
            _stateAccessors = accessors ?? throw new ArgumentNullException(nameof(accessors)); 

            _dialogs = new DialogSet();
            _dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            _dialogs.Add("Calendar_Add", new WaterfallStep[] { AskReminderTitle, SaveReminder });
            _dialogs.Add("Calendar_Find", new WaterfallStep[] { ShowReminders, ConfirmShow });
            _dialogs.Add("TitlePrompt", new TextPrompt(TitleValidator));
            _dialogs.Add("ShowReminderPrompt", new ChoicePrompt(Microsoft.Recognizers.Text.Culture.English));
        }

        private async Task TitleValidator(ITurnContext context, TextResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Value) || result.Value.Length < 3)
            {
                result.Status = PromptStatus.NotRecognized;
                await context.SendActivityAsync("Title should be at least 3 characters long.");
            }
        }

        private Task DefaultDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            return dialogContext.Context.SendActivityAsync("Hi! I'm a simple reminder bot. I can add reminders and show them.");
        }

        private async Task AskReminderTitle(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var dialogState = await dialogContext.Context.GetStateAsync<Dictionary<string, object>>(_stateAccessors.UserDialogState);
            if (dialogState.ContainsKey("Title"))
            {
                await dialogContext.ContinueAsync();
            }
            else
            {
                await dialogContext.PromptAsync("TitlePrompt", "What would you like to call your reminder?");
            }
        }

        private async Task SaveReminder(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var title = "";
            if (args is TextResult textResult)
            {
                title = textResult.Value;
            }

            // Update existing property

            var titles = await dialogContext.Context.GetStateAsync<List<Reminder>>(_stateAccessors.Reminders);

            titles.Add(new Reminder() { Title = title } );
            await dialogContext.Context.SetStateAsync<List<Reminder>>(_stateAccessors.Reminders, titles);


            await dialogContext.Context.SendActivityAsync($"Your reminder named '{title}' is set.");
            await dialogContext.EndAsync();
        }

        private async Task ShowReminders(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var titles = await dialogContext.Context.GetStateAsync<List<Reminder>>(_stateAccessors.Reminders);

            var choices = titles.Select(x => new Choice() { Value = x.Title.Length < 15 ? x.Title : x.Title.Substring(0, 15) + "..." }).ToList();
            await dialogContext.PromptAsync("ShowReminderPrompt", "Select the reminder to show: ", new ChoicePromptOptions() { Choices = choices });
        }

        private async Task ConfirmShow(DialogContext dialogContext, object args, SkipStepFunction next)
        {

            if (args is ChoiceResult choice)
            {
                var reminders = await dialogContext.Context.GetStateAsync<List<Reminder>>(_stateAccessors.Reminders);
                var reminder = ((List<Reminder>)reminders)[choice.Value.Index];

                await dialogContext.Context.SendActivityAsync($"Reminder: {reminder.Title}");
            }
            await dialogContext.EndAsync();
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new System.ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync("Hi! I'm a simple reminder bot. I can add reminders and show them.");
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogState = await turnContext.GetStateAsync<Dictionary<string, object>>(_stateAccessors.UserDialogState);

                var dialogContext = _dialogs.CreateContext(turnContext, dialogState);

                var utterance = turnContext.Activity.Text.ToLowerInvariant();
                if (utterance == "cancel")
                {
                    if (dialogContext.ActiveDialog != null)
                    {
                        await turnContext.SendActivityAsync("Ok... Cancelled");
                        dialogContext.EndAll();
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("Nothing to cancel.");
                    }
                }

                if (!turnContext.Responded)
                {
                    await dialogContext.ContinueAsync();

                    if (!turnContext.Responded)
                    {
                        var luisResult = await Startup.LuisRecognizer.RecognizeAsync(turnContext, CancellationToken.None);
                        var (intent, score) = luisResult.GetTopScoringIntent();
                        var intentResult = score > LUIS_INTENT_THRESHOLD ? intent : "None";
                        await dialogContext.BeginAsync(intent);
                    }
                }
            }
        }
    }
}
