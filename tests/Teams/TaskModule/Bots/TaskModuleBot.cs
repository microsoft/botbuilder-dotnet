// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TaskModuleBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Attachment(this.GetHeroCard());
            await turnContext.SendActivityAsync(reply);
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsTaskModuleFetchAsync Value: " + turnContext.Activity.Value.ToString());
            await turnContext.SendActivityAsync(reply);

            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = this.GetAdaptiveCard(),
                        Height = 200,
                        Width = 400,
                        Title = "Adaptive Card: Inputs",
                    },
                },
            };
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var value = turnContext.Activity.Value.ToString();
            var reply = MessageFactory.Text("OnTeamsTaskModuleFetchAsync Value: " + value);
            await turnContext.SendActivityAsync(reply);

            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = "Thanks!",
                },
            };
        }

        private Attachment GetHeroCard()
        {
            return new HeroCard()
            {
                Title = "Task Module Invocation from Hero Card",
                Subtitle = "This is a hero card with a Task Module Action button.  Click the button to show an Adaptive Card within a Task Module.",
                Buttons = new List<CardAction>()
                    {
                        new TaskModuleAction("Adaptive Card", new { data = "adaptivecard" }),
                    },
            }.ToAttachment();
        }

        private Attachment GetAdaptiveCard()
        {
            return new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock() { Text = "Enter Text Here" },
                    new AdaptiveTextInput()
                    {
                        Id = "usertext",
                        Spacing = AdaptiveSpacing.None,
                        IsMultiline = true,
                        Placeholder = "add some text and submit",
                    },
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveSubmitAction() { Title = "Submit" },
                },
            }.ToAttachment();
        }
    }
}
