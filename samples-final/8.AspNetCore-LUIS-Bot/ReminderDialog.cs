// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace AspNetCore_LUIS_Bot
{
    public class ReminderDialog : DialogContainer
    {        
        private readonly MyBotAccessors _stateAccessors;

        public ReminderDialog(MyBotAccessors accessors) : base("None")
        {
            _stateAccessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            
            Dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            Dialogs.Add("Calendar_Add", new WaterfallStep[] { AskReminderTitle, SaveReminder });
            Dialogs.Add("Calendar_Find", new WaterfallStep[] { ShowReminders, ConfirmShow });
            Dialogs.Add("TitlePrompt", new TextPrompt(TitleValidator));
            Dialogs.Add("ShowReminderPrompt", new ChoicePrompt(Microsoft.Recognizers.Text.Culture.English));
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

            titles.Add(new Reminder() { Title = title });
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
    }
}
