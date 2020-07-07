// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.BotBuilderSamples.TeamsSkillBot.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.TeamsSkillBot.Bots
{
    public class TeamsBot : TeamsActivityHandler
    {
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            // Handle invoke triggers for the skill.
            switch (turnContext.Activity.Name)
            {
                case "TeamsTaskModule":
                {
                    var reply = MessageFactory.Attachment(GetTaskModuleHeroCard());
                    await turnContext.SendActivityAsync(reply, cancellationToken);

                    //var token = await (turnContext.Adapter as SkillAdapterWithErrorHandler).GetBotSkillToken(turnContext);

                    return new InvokeResponse
                    {
                        Status = (int)HttpStatusCode.OK,
                    };
                }

                case "TeamsCardAction":
                {
                    var reply = MessageFactory.Attachment(GetAdaptiveCardWithInvokeAction());
                    await turnContext.SendActivityAsync(reply, cancellationToken);

                    return new InvokeResponse
                    {
                        Status = (int)HttpStatusCode.OK,
                    };
                }

                default:
                    // Let the base handle it.
                    return await base.OnInvokeActivityAsync(turnContext, cancellationToken);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Attachment(GetTaskModuleHeroCard());
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsTaskModuleFetchAsync TaskModuleRequest: " + JsonConvert.SerializeObject(taskModuleRequest));

            await turnContext.SendActivityAsync(reply, cancellationToken);

            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = CreateAdaptiveCardAttachment(),
                        Height = 200,
                        Width = 400,
                        Title = "Adaptive Card: Inputs",
                    },
                },
            };
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsTaskModuleSubmitAsync Value: " + JsonConvert.SerializeObject(taskModuleRequest));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            // Send End of conversation at the end.
            var activity = new Activity(ActivityTypes.EndOfConversation)
            {
                Value = taskModuleRequest.Data,
                Locale = ((Activity)turnContext.Activity).Locale
            };
            await turnContext.SendActivityAsync(activity, cancellationToken);

            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse
                {
                    Value = "Thanks!",
                },
            };
        }

        protected override async Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("hello from OnTeamsCardActionInvokeAsync."), cancellationToken);

            // Send End of conversation at the end.
            var activity = new Activity(ActivityTypes.EndOfConversation)
            {
                Locale = ((Activity)turnContext.Activity).Locale
            };
            await turnContext.SendActivityAsync(activity, cancellationToken);

            return new InvokeResponse { Status = (int)HttpStatusCode.OK };
        }

        private Attachment GetTaskModuleHeroCard()
        {
            return new HeroCard
            {
                Title = "Task Module Invocation from Hero Card",
                Subtitle = "This is a hero card with a Task Module Action button.  Click the button to show an Adaptive Card within a Task Module.",
                Buttons = new List<CardAction>
                {
                    new TaskModuleAction("Adaptive Card", new { data = "adaptivecard" }),
                },
            }.ToAttachment();
        }

        private Attachment GetAdaptiveCardWithInvokeAction()
        {
            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock("Bot Builder Invoke Action"));
            var action4 = new CardAction("invoke", "invoke", null, null, null, JObject.Parse(@"{ ""key"" : ""value"" }"));
            adaptiveCard.Actions.Add(action4.ToAdaptiveCardAction());

            return adaptiveCard.ToAttachment();
        }

        private Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths =
            {
                ".",
                "Resources",
                "adaptiveCard.json"
            };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}
