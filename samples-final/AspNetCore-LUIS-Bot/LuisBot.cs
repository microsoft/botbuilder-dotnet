// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using Prompts = Microsoft.Bot.Builder.Prompts;
using Microsoft.Recognizers.Text;
using System.Linq;

namespace AspNetCore_LUIS_Bot
{
    public class LuisBot : IBot
    {
        private const double LUIS_INTENT_THRESHOLD = 0.2d;

        private readonly DialogSet dialogs;

        public LuisBot()
        {
            dialogs = new DialogSet();
            dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            dialogs.Add("Calendar_Add", new WaterfallStep[] { AskReminderTitle, SaveReminder });
            dialogs.Add("Calendar_Find", new WaterfallStep[] { ShowReminders, ConfirmShow });
            dialogs.Add("TitlePrompt", new TextPrompt(TitleValidator));
            dialogs.Add("ShowReminderPrompt", new ChoicePrompt(Culture.English));
        }

        private async Task TitleValidator(ITurnContext context, Prompts.TextResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Value) || result.Value.Length < 3)
            {
                result.Status = Prompts.PromptStatus.NotRecognized;
                await context.SendActivity("Title should be at least 3 characters long.");
            }
        }

        private Task DefaultDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            return dialogContext.Context.SendActivity("Hi! I'm a simple reminder bot. I can add reminders and show them.");
        }

        private async Task AskReminderTitle(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var reminder = new Reminder(dialogContext.ActiveDialog.State);
            dialogContext.ActiveDialog.State = reminder;
            if (!string.IsNullOrEmpty(reminder.Title))
            {
                await dialogContext.Continue();
            }
            else
            {
                await dialogContext.Prompt("TitlePrompt", "What would you like to call your reminder?");
            }
        }

        private async Task SaveReminder(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var reminder = new Reminder(dialogContext.ActiveDialog.State);
            if (args is Prompts.TextResult textResult)
            {
                reminder.Title = textResult.Value;
            }
            await dialogContext.Context.SendActivity($"Your reminder named '{reminder.Title}' is set.");
            var userContext = dialogContext.Context.GetUserState<UserState>();
            userContext.Reminders.Add(reminder);
            await dialogContext.End();
        }

        private async Task ShowReminders(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var userContext = dialogContext.Context.GetUserState<UserState>();
            if (userContext.Reminders.Count == 0)
            {
                await dialogContext.Context.SendActivity("No reminders found.");
                await dialogContext.End();
            }
            else
            {
                var choices = userContext.Reminders.Select(x => new Prompts.Choices.Choice() { Value = x.Title.Length < 15 ? x.Title : x.Title.Substring(0, 15) + "..." }).ToList();
                await dialogContext.Prompt("ShowReminderPrompt", "Select the reminder to show: ", new ChoicePromptOptions() { Choices = choices });
            }
        }

        private async Task ConfirmShow(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var userContext = dialogContext.Context.GetUserState<UserState>();
            if (args is Prompts.ChoiceResult choice)
            {
                var reminder = userContext.Reminders[choice.Value.Index];
                await dialogContext.Context.SendActivity($"Reminder: {reminder.Title}");
            }
            await dialogContext.End();
        }
        
        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.ConversationUpdate && context.Activity.MembersAdded.FirstOrDefault()?.Id == context.Activity.Recipient.Id)
            {
                await context.SendActivity("Hi! I'm a simple reminder bot. I can add reminders and show them.");
            }
            else if (context.Activity.Type == ActivityTypes.Message)
            {
                var userState = context.GetUserState<UserState>();
                if (userState.Reminders == null)
                {
                    userState.Reminders = new List<Reminder>();
                }

                var state = context.GetConversationState<Dictionary<string, object>>();
                var dialogContext = dialogs.CreateContext(context, state);

                var utterance = context.Activity.Text.ToLowerInvariant();
                if (utterance == "cancel")
                {
                    if (dialogContext.ActiveDialog != null)
                    {
                        await context.SendActivity("Ok... Cancelled");
                        dialogContext.EndAll();
                    }
                    else
                    {
                        await context.SendActivity("Nothing to cancel.");
                    }
                }
                
                if (!context.Responded)
                {
                    await dialogContext.Continue();

                    if (!context.Responded)
                    {
                        var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
                        var (intent, score) = luisResult.GetTopScoringIntent();
                        var intentResult = score > LUIS_INTENT_THRESHOLD ? intent : "None";

                        await dialogContext.Begin(intent);
                    }
                }
            }
        }
    }
}
