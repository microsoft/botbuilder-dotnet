// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class Alarm
    {
        public string Title { get; set; }

        public DateTime Time { get; set; }
    }

    public class AlarmPromptOptions : DialogOptions
    {
        public Alarm InitialAlarm { get; set; }
    }

    public class AlarmPrompt : ComponentDialog
    {
        private const string AlarmTitle = "title";
        private const string AlarmTime = "time";
        private const string InitialDialog = "start";
        private const string TitlePrompt = "titlePrompt";
        private const string TimePrompt = "timePrompt";

        public AlarmPrompt(string dialogId)
            : base(dialogId)
        {
            // Add control flow dialogs to components dialog set
            var ws = new WaterfallStep[]
            {
                InitializeValuesStepAsync,
                AskTitleStepAsync,
                AskTimeStepAsync,
                ReturnAlarmStepAsync,
            };
            AddDialog(new WaterfallDialog(InitialDialog, ws));

            // Add dialogs for prompts
            AddDialog(new TextPrompt(TitlePrompt, ValidateTitleAsync));
            AddDialog(new DateTimePrompt(TimePrompt));
        }

        private async Task ValidateTitleAsync(ITurnContext context, PromptValidatorContext<string> prompt)
        {
            var value = (prompt.Recognized.Value ?? string.Empty).Trim();
            if (value.Length < 3)
            {
                // Send error message to user
                await context.SendActivityAsync("The title must be at least 3 characters long.").ConfigureAwait(false);
            }
            else
            {
                // Return value to caller
                prompt.End(value);
            }
        }

        private async Task<DialogTurnResult> InitializeValuesStepAsync(DialogContext dc, WaterfallStepContext step)
        {
            // Populate Values dictionary with any initial values.
            if (step.Options is AlarmPromptOptions options)
            {
                if (options.InitialAlarm != null)
                {
                    if (!string.IsNullOrEmpty(options.InitialAlarm.Title))
                    {
                        step.Values[AlarmTitle] = options.InitialAlarm.Title;
                    }

                    if (options.InitialAlarm.Time != null)
                    {
                        step.Values[AlarmTime] = options.InitialAlarm.Time;
                    }
                }
            }

            // Call next step.
            return await step.NextAsync().ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> AskTitleStepAsync(DialogContext dc, WaterfallStepContext step)
        {
            // Prompt for title if missing
            if (!step.Values.ContainsKey(AlarmTitle))
            {
                return await dc.PromptAsync(TitlePrompt, "What would you like to call your alarm?").ConfigureAwait(false);
            }
            else
            {
                return await step.NextAsync().ConfigureAwait(false);
            }
        }

        private async Task<DialogTurnResult> AskTimeStepAsync(DialogContext dc, WaterfallStepContext step)
        {
            // Save title if prompted for.
            if (step.Result != null)
            {
                step.Values[AlarmTitle] = step.Result;
            }

            // Prompt for time if missing.
            if (!step.Values.ContainsKey(AlarmTime))
            {
                return await dc.PromptAsync(TimePrompt, $"What time would you like your '{step.Values[AlarmTitle]}' alarm set for?").ConfigureAwait(false);
            }
            else
            {
                return await step.NextAsync().ConfigureAwait(false);
            }
        }

        private async Task<DialogTurnResult> ReturnAlarmStepAsync(DialogContext dc, WaterfallStepContext step)
        {
            // Save time if prompted for.
            if (step.Result != null)
            {
                step.Values[AlarmTime] = step.Result;
            }

            // Format alarm and return to caller.
            var alarm = new Alarm
            {
                Title = (string)step.Values[AlarmTitle],
                Time = (DateTime)step.Values[AlarmTime],
            };
            return await dc.EndAsync(alarm).ConfigureAwait(false);
        }
    }
}
