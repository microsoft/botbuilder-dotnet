// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Skills.Dialogs;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.DialogRootBot31.Dialogs
{
    /// <summary>
    /// The main dialog for this bot. It uses a <see cref="SkillDialog"/> to call skills.
    /// </summary>
    public class MainDialog : ComponentDialog
    {
        private readonly string _selectedSkillKey = $"{typeof(MainDialog).FullName}.SelectedSkillKey";
        private readonly SkillsConfiguration _skillsConfig;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ConversationState conversationState, SkillConversationIdFactoryBase conversationIdFactory, SkillHttpClient skillClient, SkillsConfiguration skillsConfig, IConfiguration configuration)
            : base(nameof(MainDialog))
        {
            var botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            if (string.IsNullOrWhiteSpace(botId))
            {
                throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is not in configuration");
            }

            _skillsConfig = skillsConfig ?? throw new ArgumentNullException(nameof(skillsConfig));

            if (skillClient == null)
            {
                throw new ArgumentNullException(nameof(skillClient));
            }

            if (conversationState == null)
            {
                throw new ArgumentNullException(nameof(conversationState));
            }

            // ChoicePrompt to render available skills and skill actions
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // SkillDialog used to wrap interaction with the selected skill
            var skillDialogOptions = new SkillDialogOptions
            {
                BotId = botId,
                ConversationIdFactory = conversationIdFactory,
                SkillClient = skillClient,
                SkillHostEndpoint = skillsConfig.SkillHostEndpoint
            };
            AddDialog(new SkillDialog(skillDialogOptions, conversationState));

            // Main waterfall dialog for this bot
            var waterfallSteps = new WaterfallStep[]
            {
                SelectSkillStepAsync,
                SelectSkillActionStepAsync,
                CallSkillActionStepAsync,
                FinalStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Render a prompt to select the skill to call.
        private async Task<DialogTurnResult> SelectSkillStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create the PromptOptions from the skill configuration which contain the list of configured skills.
            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text("What skill would you like to call?"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a valid skill."),
                Choices = _skillsConfig.Skills.Select(skill => new Choice(skill.Value.Id)).ToList()
            };

            // Prompt the user to select a skill.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        // Render a prompt to select the action for the skill.
        private async Task<DialogTurnResult> SelectSkillActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the skill info based on the selected skill.
            var selectedSkillId = ((FoundChoice)stepContext.Result).Value;
            var selectedSkill = _skillsConfig.Skills.FirstOrDefault(s => s.Value.Id == selectedSkillId).Value;

            // Remember the skill selected by the user.
            stepContext.Values[_selectedSkillKey] = selectedSkill;

            // Create the PromptOptions with the actions supported by the selected skill.
            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text($"What action would you like to call in **{selectedSkill.Id}**?"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a valid action."),
                Choices = GetSkillActions(selectedSkill),
                Style = ListStyle.SuggestedAction
            };

            // Prompt the user to select a skill action.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        // Starts SkillDialog based on the user's selections
        private async Task<DialogTurnResult> CallSkillActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selectedSkill = (BotFrameworkSkill)stepContext.Values[_selectedSkillKey];

            var skillDialogArgs = new SkillDialogArgs();
            switch (selectedSkill.Id)
            {
                case "EchoSkillBot":
                    // Echo skill only handles message activities, send a dummy utterance to get it started.
                    skillDialogArgs = new SkillDialogArgs
                    {
                        ActivityType = ActivityTypes.Message,
                        Text = "Start echo skill"
                    };
                    break;
                case "DialogSkillBot":
                    skillDialogArgs = GetDialogSkillBotArgs(((FoundChoice)stepContext.Result).Value);
                    break;
            }

            // Set the skill to invoke in the dialogArgs
            skillDialogArgs.Skill = selectedSkill;

            // Start the skillDialog with the arguments. 
            return await stepContext.BeginDialogAsync(nameof(SkillDialog), skillDialogArgs, cancellationToken);
        }

        // The SkillDialog has ended, render the results (if any) and restart MainDialog.
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result != null)
            {
                var message = "Skill invocation complete.";
                message += $" Result: {JsonConvert.SerializeObject(stepContext.Result)}";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message, inputHint: InputHints.IgnoringInput), cancellationToken: cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "What else can I do for you?", cancellationToken);
        }

        // Helper method to create Choice elements for the actions supported by the skill
        private IList<Choice> GetSkillActions(BotFrameworkSkill skill)
        {
            // Note: the bot would probably render this by readying the skill manifest
            // we are just using hardcoded skill actions here for simplicity.

            var choices = new List<Choice>();
            switch (skill.Id)
            {
                case "EchoSkillBot":
                    choices.Add(new Choice("Messages"));
                    break;

                case "DialogSkillBot":
                    choices.Add(new Choice("m:some message for tomorrow"));
                    choices.Add(new Choice("BookFlight"));
                    choices.Add(new Choice("OAuthTest"));
                    choices.Add(new Choice("mv:some message with value"));
                    choices.Add(new Choice("BookFlightWithValues"));
                    break;
            }

            return choices;
        }

        private SkillDialogArgs GetDialogSkillBotArgs(string selectedOption)
        {
            // Note: in a real bot, the dialogArgs will be created dynamically based on the conversation
            // and what each action requires, this code hardcodes the values to make things simpler.

            // Send a message activity to the skill.
            if (selectedOption.StartsWith("m:", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    ActivityType = ActivityTypes.Message,
                    Text = selectedOption.Substring(2).Trim()
                };
                return dialogArgs;
            }

            // Send a message activity to the skill with some artificial parameters in value
            if (selectedOption.StartsWith("mv:", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    ActivityType = ActivityTypes.Message,
                    Text = selectedOption.Substring(3).Trim(),
                    Value = new BookingDetails { Destination = "New York" }
                };
                return dialogArgs;
            }

            // Send an event activity to the skill with "OAuthTest" in the name.
            if (selectedOption.Equals("OAuthTest", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    ActivityType = ActivityTypes.Event,
                    Name = "OAuthTest"
                };
                return dialogArgs;
            }

            // Send an event activity to the skill with "BookFlight" in the name.
            if (selectedOption.Equals("BookFlight", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    ActivityType = ActivityTypes.Event,
                    Name = "BookFlight"
                };
                return dialogArgs;
            }

            // Send an event activity to the skill "BookFlight" in the name and some testing values.
            if (selectedOption.Equals("BookFlightWithValues", StringComparison.CurrentCultureIgnoreCase))
            {
                var dialogArgs = new SkillDialogArgs
                {
                    ActivityType = ActivityTypes.Event,
                    Name = "BookFlight",
                    Value = new BookingDetails
                    {
                        Destination = "New York",
                        Origin = "Seattle"
                    }
                };
                return dialogArgs;
            }

            throw new Exception($"Unable to create dialogArgs for \"{selectedOption}\".");
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
