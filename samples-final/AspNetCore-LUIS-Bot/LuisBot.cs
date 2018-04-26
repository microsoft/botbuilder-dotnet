using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using Prompts = Microsoft.Bot.Builder.Prompts;
using Newtonsoft.Json.Linq;
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
            dialogs.Add("Fallback", new WaterfallStep[] { DefaultDialog });
            dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            dialogs.Add("Calendar.Add", new WaterfallStep[] { AskRemainderTitle, AskRemainderTime, AskRemainderConfirmation });
            dialogs.Add("Calendar.Delete", new WaterfallStep[] { DeleteReminder, ConfirmDelete });
            dialogs.Add("Calendar.Find", new WaterfallStep[] { ShowReminders, ConfirmShow });
            dialogs.Add("TitlePrompt", new TextPrompt(TitleValidator));
            dialogs.Add("DatetimePrompt", new TextPrompt(DatetimeValidator));
            dialogs.Add("DeletePrompt", new ConfirmPrompt(Culture.English));
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

        private async Task DatetimeValidator(ITurnContext context, Prompts.TextResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.Value) && DateTime.TryParse(result.Value, out DateTime datetime))
            {
                result.Value = datetime.ToString();
            }
            else
            {
                result.Status = Prompts.PromptStatus.NotRecognized;
                await context.SendActivity("Please enter a valid time in the future like \"tomorrow at 9am\" or say \"cancel\".");
            }
        }

        private Task DefaultDialog(DialogContext dc, object args, SkipStepFunction next)
        {
            return dc.Context.SendActivity("Hi! I'm a simple reminder bot. I can help add reminders, delete and show them.");
        }

        private async Task AskRemainderTitle(DialogContext dc, object args, SkipStepFunction next)
        {
            var initState = dc.Instance.State as WaterfallInstance;
            var state = new Reminder()
            {
                Step = initState.Step
            };
            if (args is RecognizerResult luisResult)
            {
                var title = GetEntity<string>(luisResult, "Calendar_Subject");
                if (!string.IsNullOrEmpty(title))
                {
                    state.Title = title;
                }

                var date = GetEntity<DateTime?>(luisResult, "datetimeV2");
                if (date.HasValue)
                {
                    state.Date = date;
                }
            }
            dc.Instance.State = state;
            if (string.IsNullOrEmpty(state.Title))
            {
                await dc.Prompt("TitlePrompt", "What would you like to call your reminder?");
            }
            else
            {
                await dc.Continue();
            }
        }

        private async Task AskRemainderTime(DialogContext dc, object args, SkipStepFunction next)
        {
            var state = dc.Instance.State as Reminder;
            if (string.IsNullOrEmpty(state.Title))
            {
                if (args is Prompts.TextResult textResult)
                {
                    state.Title = textResult.Value;
                }
            }
            if (!state.Date.HasValue)
            {
                await dc.Prompt("DatetimePrompt", $"What time would you like to set the {state.Title} reminder for?");
            }
            else
            {
                await dc.Continue();
            }
        }

        private async Task AskRemainderConfirmation(DialogContext dc, object args, SkipStepFunction next)
        {
            var state = dc.Instance.State as Reminder;
            if (state.Date == null)
            {
                if (args is Prompts.TextResult textResult)
                {
                    state.Date = DateTime.Parse(textResult.Value);
                }
            }
            await dc.Context.SendActivity($"Your reminder named '{state.Title}' is set for {state.Date}.");
            var userContext = dc.Context.GetUserState<UserState>();
            userContext.Reminders.Add(state);
            dc.Instance.State = state as WaterfallInstance;
            await dc.End();
        }

        private async Task DeleteReminder(DialogContext dc, object args, SkipStepFunction next)
        {
            var userContext = dc.Context.GetUserState<UserState>();
            if (userContext.Reminders.Count > 0)
            {
                await dc.Prompt("DeletePrompt", "Are you sure you want to delete the reminders?");
            }
            else
            {
                await dc.Context.SendActivity("No reminders set to delete.");
                await dc.End();
            }
        }

        private async Task ConfirmDelete(DialogContext dc, object args, SkipStepFunction next)
        {
            var userContext = dc.Context.GetUserState<UserState>();
            if (args is Prompts.ConfirmResult confirmResult)
            {
                if (confirmResult.Confirmation)
                {
                    userContext.Reminders.Clear();
                    await dc.Context.SendActivity("reminders deleted...");
                }
                else
                {
                    await dc.Context.SendActivity("ok...");
                }
            }
            await dc.End();
        }

        private async Task ShowReminders(DialogContext dc, object args, SkipStepFunction next)
        {
            var userContext = dc.Context.GetUserState<UserState>();
            if (userContext.Reminders.Count == 0)
            {
                await dc.Context.SendActivity("No reminders found.");
                await dc.End();
            }
            else
            {
                var choices = userContext.Reminders.Select(x => new Prompts.Choices.Choice() { Value = x.Title }).ToList();
                await dc.Prompt("ShowReminderPrompt", "Select the reminder to show: ", new ChoicePromptOptions() { Choices = choices });
            }
        }

        private async Task ConfirmShow(DialogContext dc, object args, SkipStepFunction next)
        {
            var userContext = dc.Context.GetUserState<UserState>();
            if (args is Prompts.ChoiceResult choice)
            {
                var reminder = userContext.Reminders.FirstOrDefault(x => x.Title == choice.Value.Value);
                await dc.Context.SendActivity($"Reminder: {reminder.Title} - ({reminder.Date})");
            }
            await dc.End();
        }

        private T GetEntity<T>(RecognizerResult luisResult, string entityKey)
        {
            var data = luisResult.Entities as IDictionary<string, JToken>;
            if (data.TryGetValue(entityKey, out JToken value))
            {
                return value.First.Value<T>();
            }
            return default(T);
        }

        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                await context.SendActivity("Hi! I'm a simple reminder bot. I can help add reminders, delete and show them.");
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
                    if (dialogContext.Instance != null)
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
                        var intentResult = score > LUIS_INTENT_THRESHOLD ? intent : "Fallback";

                        await dialogContext.Begin(intent, luisResult);
                    }
                }
            }
        }
    }
}
