// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TaskModuleBot : TeamsActivityHandler
    {
        string _webRootPath;

        public TaskModuleBot(IHostingEnvironment hostingEnvironment)
        {
            _webRootPath = hostingEnvironment.ContentRootPath;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard()
            {
                Title = "Task Module Invocation from Hero Card",
                Subtitle = "This is a hero card with a Task Module Action button.  Click the button to show an Adaptive Card within a Task Module.",
                Buttons = new List<CardAction>()
                    {
                        new TaskModuleAction("Adaptive Card", new { data = "adaptivecard" }),
                    },
            };

            var reply = MessageFactory.Attachment(card.ToAttachment());
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
                        Card = AdaptiveCardHelper.GetAdaptiveCard(_webRootPath),
                        Height = 500,
                        Width = 700,
                        Title = "Adaptive Card: Inputs",
                    },
                },
            };
        }

        protected override async Task OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsTaskModuleSubmitAsync Value: " + turnContext.Activity.Value.ToString());
            await turnContext.SendActivityAsync(reply);
        }
    }
}
