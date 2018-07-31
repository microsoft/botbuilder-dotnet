// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration;
// using Microsoft.Recognizers.Text;

namespace AspNetCore_LUIS_Bot
{
    public class LuisBot :  IBot
    {
        private const double LUIS_INTENT_THRESHOLD = 0.2d;

        private readonly DialogSet _dialogs;
        private readonly Dictionary<string, JObject> _dictionary;

        // Property Accessors
        private readonly IStatePropertyAccessor<Dictionary<string, object>> _userDialogState;
        private readonly IStatePropertyAccessor<List<string>> _reminderTitles;
        public LuisBot()
        {
        }

        public LuisBot(BotFrameworkOptions options)
        {
            // Create Property Accessors
            foreach (var state in options.State)
            {
                if (state is UserState)
                {
                    _reminderTitles = state.CreateProperty<List<string>>(UserStateProperty.ReminderTitles, () => new List<string>());
                    _userDialogState = state.CreateProperty<Dictionary<string, object>>(UserStateProperty.DialogState, () => new Dictionary<string, object>());
                }
                if (state is ConversationState)
                {
                }
            }

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
            var dialogState = await dialogContext.Context.GetStateAsync<Dictionary<string, object>>(_userDialogState);
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

            var titles = await dialogContext.Context.GetStateAsync<List<string>>(_reminderTitles);
            titles.Add(title);
            await dialogContext.Context.SetStateAsync<List<string>>(_reminderTitles, titles);
            

            await dialogContext.Context.SendActivityAsync($"Your reminder named '{title}' is set.");
            await dialogContext.EndAsync();
        }

        private async Task ShowReminders(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var titles = await dialogContext.Context.GetStateAsync<List<string>>(_reminderTitles);

            var choices = titles.Select(x => new Choice() { Value = x.Length < 15 ? x : x.Substring(0, 15) + "..." }).ToList();
            await dialogContext.PromptAsync("ShowReminderPrompt", "Select the reminder to show: ", new ChoicePromptOptions() { Choices = choices });
        }

        private async Task ConfirmShow(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            
            if (args is ChoiceResult choice)
            {
                var reminders = await dialogContext.Context.GetStateAsync<List<string>>(_reminderTitles);
                string reminder = ((List<string>) reminders)[choice.Value.Index];

                
                await dialogContext.Context.SendActivityAsync($"Reminder: {reminder}");
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
                var dialogState = await turnContext.GetStateAsync<Dictionary<string, object>>(_userDialogState);

                var dialogContext = _dialogs.CreateContext(turnContext,  dialogState);

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
