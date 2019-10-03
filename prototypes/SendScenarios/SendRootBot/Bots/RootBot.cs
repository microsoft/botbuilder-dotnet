// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace SendRootBot.Bots
{
    public class RootBot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly IStatePropertyAccessor<Dictionary<string, object>> _convoState;
        private readonly SkillConnector _skillConnector;

        public RootBot(ConversationState conversationState, SkillConnector skillConnector)
        {
            _skillConnector = skillConnector;
            _conversationState = conversationState;
            _convoState = conversationState.CreateProperty<Dictionary<string, object>>("CurrentTask");
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var state = await _convoState.GetAsync(turnContext, () => new Dictionary<string, object>(), cancellationToken);
            SkillTurnResult ret;
            if (state.ContainsKey("activeFlow") && state["activeFlow"] != null)
            {
                // We have an active flow, keep sending activities there.
                ret = await _skillConnector.ProcessActivityAsync(turnContext, (Activity)turnContext.Activity, cancellationToken);
            }
            else
            {
                // We don't have a flow, figure out what to invoke based on the selected scenario
                switch (turnContext.Activity.Text)
                {
                    case "Book a Flight":
                        state["activeFlow"] = "Book a Flight";
                        var bookFlightActivity = (Activity)turnContext.Activity;
                        bookFlightActivity.SemanticAction = new SemanticAction("BookFlight");
                        ret = await _skillConnector.ProcessActivityAsync(turnContext, bookFlightActivity, cancellationToken);
                        break;

                    case "Book a Flight (With Data)":
                        state["activeFlow"] = "Book a Flight (With Data)";
                        var bookFlightActivityWithData = (Activity)turnContext.Activity;
                        bookFlightActivityWithData.SemanticAction = new SemanticAction("BookFlight")
                        {
                            Entities = new Dictionary<string, Entity>
                            {
                                { "bookingInfo", new Entity() },
                            },
                        };
                        bookFlightActivityWithData.SemanticAction.Entities["bookingInfo"].SetAs(new BookingDetails()
                        {
                            Destination = "NY",
                            Origin = "SEA",
                            TravelDate = $"{DateTime.Now.AddDays(2):yyyy-MM-dd}",
                        });

                        ret = await _skillConnector.ProcessActivityAsync(turnContext, bookFlightActivityWithData, cancellationToken);
                        break;

                    case "Get weather":
                        state["activeFlow"] = "Get weather";
                        var getWeatherActivity = (Activity)turnContext.Activity;
                        getWeatherActivity.SemanticAction = new SemanticAction("GetWeather");
                        ret = await _skillConnector.ProcessActivityAsync(turnContext, getWeatherActivity, cancellationToken);
                        break;

                    case "SendAsIs":
                        state["activeFlow"] = "SendAsIs";
                        ret = await _skillConnector.ProcessActivityAsync(turnContext, turnContext.Activity as Activity, cancellationToken);
                        break;

                    default:
                        await turnContext.SendActivityAsync(MessageFactory.Text("Didn't get that (from RootBot)"), cancellationToken);
                        return;
                }
            }

            // Check if the remote skill ended.
            if (ret.Status == SkillTurnStatus.Complete)
            {
                // Evaluate return value.
                if (ret.Result != null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"The skill has ended with return value = {JsonConvert.SerializeObject(ret.Result)}"), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("The skill has ended"), cancellationToken);
                }

                await SendMainMenuAsync(turnContext, cancellationToken);

                // Clear active flow state
                state["activeFlow"] = null;
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await SendMainMenuAsync(turnContext, cancellationToken);
                }
            }
        }

        private static async Task SendMainMenuAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            IEnumerable<string> actions = new List<string>
            {
                "Book a Flight",
                "Book a Flight (With Data)",
                "Get weather",
                "SendAsIs",
            };
            var msg = MessageFactory.SuggestedActions(actions, "Hello and welcome to the Send Scenarios bot!");
            await turnContext.SendActivityAsync(msg, cancellationToken);
        }

        private class BookingDetails
        {
            public string Destination { get; set; }

            public string Origin { get; set; }

            public string TravelDate { get; set; }
        }
    }
}
