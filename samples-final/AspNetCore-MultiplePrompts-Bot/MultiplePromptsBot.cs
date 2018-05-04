// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using PromptsDialog = Microsoft.Bot.Builder.Dialogs;

namespace AspNetCore_Multiple_Prompts
{
    public static class PromptStep
    {
        public const string GatherInfo = "gatherInfo";
        public const string NamePrompt = "namePrompt";
        public const string AgePrompt = "agePrompt";
    }

    public class MultiplePromptsBot : IBot
    {
        private readonly DialogSet dialogs;

        private async Task NameValidator(ITurnContext context, TextResult result)
        {
            if (result.Value.Length <= 2)
            {
                result.Status = PromptStatus.NotRecognized;
                await context.SendActivity("Your name should be at least 2 characters long.");
            }
        }

        private async Task AgeValidator(ITurnContext context, NumberResult<int> result)
        {
            if (0 > result.Value || result.Value > 122)
            {
                result.Status = PromptStatus.NotRecognized;
                await context.SendActivity("Your age should be between 0 and 122.");
            }
        }

        private async Task AskNameStep(DialogContext dialogContext, object result, SkipStepFunction next)
        {
            await dialogContext.Prompt(PromptStep.NamePrompt, "What is your name?");
        }

        private async Task AskAgeStep(DialogContext dialogContext, object result, SkipStepFunction next)
        {
            var state = dialogContext.Context.GetConversationState<MultiplePromptsState>();
            state.Name = (result as TextResult).Value;
            await dialogContext.Prompt(PromptStep.AgePrompt, "What is your age?");
        }

        private async Task GatherInfoStep(DialogContext dialogContext, object result, SkipStepFunction next)
        {
            var state = dialogContext.Context.GetConversationState<MultiplePromptsState>();
            state.Age = (result as NumberResult<int>).Value;
            await dialogContext.Context.SendActivity($"Your name is {state.Name} and your age is {state.Age}");
            await dialogContext.End();
        }

        public MultiplePromptsBot()
        {
            dialogs = new DialogSet();

            // Create prompt for name with string length validation
            dialogs.Add(PromptStep.NamePrompt,
                new PromptsDialog.TextPrompt(NameValidator));
            // Create prompt for age with number value validation
            dialogs.Add(PromptStep.AgePrompt,
                new PromptsDialog.NumberPrompt<int>(Culture.English, AgeValidator));
            // Add a dialog that uses both prompts to gather information from the user
            dialogs.Add(PromptStep.GatherInfo,
                new WaterfallStep[] { AskNameStep, AskAgeStep, GatherInfoStep });
        }

        public async Task OnTurn(ITurnContext context)
        {
            var state = context.GetConversationState<MultiplePromptsState>();
            var dialogCtx = dialogs.CreateContext(context, state);
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:
                    await dialogCtx.Continue();
                    if (!context.Responded)
                    {
                        await dialogCtx.Begin(PromptStep.GatherInfo);
                    }
                    break;
            }
        }
    }
}
