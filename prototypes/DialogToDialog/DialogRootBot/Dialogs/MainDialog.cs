// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DialogRootBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly string _botId;
        private readonly ConversationState _conversationState;
        private readonly SkillHttpClient _skillClient;
        private readonly SkillsConfiguration _skillsConfig;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ConversationState conversationState, SkillHttpClient skillClient, SkillsConfiguration skillsConfig, SkillDialog bookingDialog, IConfiguration configuration)
            : base(nameof(MainDialog))
        {
            _botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            _skillClient = skillClient;
            _skillsConfig = skillsConfig;
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(bookingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { IntroStepAsync, ActStepAsync, FinalStepAsync }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?";
            var promptMessage = CreateTaskPromptMessageWithActions(messageText);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // TODO: simplify this method.
            // Forward to skill as message.
            if (stepContext.Context.Activity.Text.StartsWith("m:", StringComparison.CurrentCultureIgnoreCase))
            {
                stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.Substring(2).Trim();
                var dialogArgs = new SkillDialogArgs { SkillId = "SkillBot" };
                return await stepContext.BeginDialogAsync(nameof(SkillDialog), dialogArgs, cancellationToken);
            }

            // Forward to skill as a message with some artificial parameters in value
            if (stepContext.Context.Activity.Text.StartsWith("mv:", StringComparison.CurrentCultureIgnoreCase))
            {
                stepContext.Context.Activity.Text = stepContext.Context.Activity.Text.Substring(3).Trim();
                var dialogArgs = new SkillDialogArgs
                {
                    SkillId = "SkillBot",
                    Value = new BookingDetails { Destination = "New York" }
                };
                return await stepContext.BeginDialogAsync(nameof(SkillDialog), dialogArgs, cancellationToken);
            }

            // Forward to skill as event with "OAuthTest" in the name.
            if (stepContext.Context.Activity.Text.Equals("OAuthTest", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    SkillId = "SkillBot",
                    EventName = "OAuthTest"
                };
                return await stepContext.BeginDialogAsync(nameof(SkillDialog), dialogArgs, cancellationToken);
            }

            // Forward to skill as event with "BookFlight" in the name.
            if (stepContext.Context.Activity.Text.Equals("BookFlight", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    SkillId = "SkillBot",
                    EventName = "BookFlight"
                };
                return await stepContext.BeginDialogAsync(nameof(SkillDialog), dialogArgs, cancellationToken);
            }

            // Forward to skill as event with "BookFlight" in the name and some testing values.
            if (stepContext.Context.Activity.Text.Equals("BookFlightWithValues", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    SkillId = "SkillBot",
                    EventName = "BookFlight",
                    Value = new BookingDetails
                    {
                        Destination = "New York",
                        Origin = "Seattle"
                    }
                };
                return await stepContext.BeginDialogAsync(nameof(SkillDialog), dialogArgs, cancellationToken);
            }

            // Forward to skill as event with "BookFlight" in the name and some testing values.
            if (stepContext.Context.Activity.Text.Equals("BookFlightWithValues", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    SkillId = "SkillBot",
                    EventName = "BookFlight",
                    Value = new BookingDetails
                    {
                        Destination = "New York",
                        Origin = "Seattle"
                    }
                };
                return await stepContext.BeginDialogAsync(nameof(SkillDialog), dialogArgs, cancellationToken);
            }

            // Forward to skill as an InvokeActivity with "GetWeather" in the name and some testing values.
            // Note that this operation doesn't use SkillDialog, InvokeActivities are single turn Request/Response.
            if (stepContext.Context.Activity.Text.Equals("GetWeather", StringComparison.CurrentCultureIgnoreCase))
            {
                var invokeActivity = Activity.CreateInvokeActivity();
                invokeActivity.Name = "GetWeather";
                invokeActivity.ApplyConversationReference(stepContext.Context.Activity.GetConversationReference());
                invokeActivity.From = stepContext.Context.Activity.From;
                invokeActivity.Recipient = stepContext.Context.Activity.Recipient;

                await _conversationState.SaveChangesAsync(stepContext.Context, true, cancellationToken);
                var response = await _skillClient.PostActivityAsync(_botId, _skillsConfig.Skills["SkillBot"], _skillsConfig.SkillHostEndpoint, (Activity)invokeActivity, cancellationToken);
                return await stepContext.NextAsync(response.Body, cancellationToken);
            }

            // Catch all for unhandled intents
            var didntUnderstandMessageText = "Sorry, I didn't get that. Please try asking in a different way.";
            var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = "Skill invocation complete.";
            if (stepContext.Result != null)
            {
                message += $" Result: {JsonConvert.SerializeObject(stepContext.Result)}";
            }

            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);

            // Restart the main dialog with a different message the second time around
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "What else can I do for you?", cancellationToken);
        }

        private Activity CreateTaskPromptMessageWithActions(string messageText)
        {
            var activity = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            activity.SuggestedActions = new SuggestedActions
            {
                Actions = new List<CardAction>
                {
                    new CardAction
                    {
                        Title = "Hi",
                        Type = ActionTypes.ImBack,
                        Value = "Hi"
                    },
                    new CardAction
                    {
                        Title = "m:some message",
                        Type = ActionTypes.ImBack,
                        Value = "m:some message for tomorrow"
                    },
                    new CardAction
                    {
                        Title = "Book a flight",
                        Type = ActionTypes.ImBack,
                        Value = "BookFlight"
                    },
                    new CardAction
                    {
                        Title = "Get Weather",
                        Type = ActionTypes.ImBack,
                        Value = "GetWeather"
                    },
                    new CardAction
                    {
                        Title = "OAuthTest",
                        Type = ActionTypes.ImBack,
                        Value = "OAuthTest"
                    },
                    new CardAction
                    {
                        Title = "mv:some message with value",
                        Type = ActionTypes.ImBack,
                        Value = "mv:some message with value"
                    },
                    new CardAction
                    {
                        Title = "Book a flight with values",
                        Type = ActionTypes.ImBack,
                        Value = "BookFlightWithValues"
                    }
                }
            };
            return activity;
        }
    }
}
