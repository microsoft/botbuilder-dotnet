// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Message activity card builder for QnAMaker dialogs.
    /// </summary>
    public static class QnACardBuilder
    {
        /// <summary>
        /// Get active learning suggestions card.
        /// </summary>
        /// <param name="suggestionsList">List of suggested questions.</param>
        /// <param name="cardTitle">Title of the cards.</param>
        /// <param name="cardNoMatchText">No match text.</param>
        /// <returns>IMessageActivity.</returns>
        public static IMessageActivity GetSuggestionsCard(List<string> suggestionsList, string cardTitle, string cardNoMatchText)
        {
            if (suggestionsList == null)
            {
                throw new ArgumentNullException(nameof(suggestionsList));
            }

            if (cardTitle == null)
            {
                throw new ArgumentNullException(nameof(cardTitle));
            }

            if (cardNoMatchText == null)
            {
                throw new ArgumentNullException(nameof(cardNoMatchText));
            }

            var chatActivity = Activity.CreateMessageActivity();
            chatActivity.Text = cardTitle;
            var buttonList = new List<CardAction>();

            // Add all suggestions
            foreach (var suggestion in suggestionsList)
            {
                buttonList.Add(
                    new CardAction()
                    {
                        Value = suggestion,
                        Type = "imBack",
                        Title = suggestion,
                    });
            }

            // Add No match text
            buttonList.Add(
                new CardAction()
                {
                    Value = cardNoMatchText,
                    Type = "imBack",
                    Title = cardNoMatchText
                });

            var plCard = new HeroCard()
            {
                Buttons = buttonList
            };

            // Create the attachment.
            var attachment = plCard.ToAttachment();

            chatActivity.Attachments.Add(attachment);

            return chatActivity;
        }

        /// <summary>
        /// Get Card for MultiTurn scenario. (Can be deprecated from 4.10.0 release of sdk).
        /// </summary>
        /// <param name="result">Result to be dispalyed as prompts.</param>
        /// <param name="cardNoMatchText">No match text.</param>
        /// <returns>IMessageActivity.</returns>
#pragma warning disable CA1801 // Review unused parameters (we can't remove cardNoMatchText without breaking binary compat) 
        public static IMessageActivity GetQnAPromptsCard(QueryResult result, string cardNoMatchText = "")
#pragma warning restore CA1801 // Review unused parameters
        {
            return GetQnADefaultResponse(result, true);
        }

        /// <summary>
        /// Get Card for Default QnA Maker scenario.
        /// </summary>
        /// <param name="result">Result to be dispalyed as prompts.</param>
        /// <param name="displayPreciseAnswerOnly">Choice to render precise answer.</param>
        /// <param name="useTeamsAdaptiveCard">Choose whether to use a Teams-formatted Adaptive card.</param>
        /// <returns>IMessageActivity.</returns>
        public static IMessageActivity GetQnADefaultResponse(QueryResult result, BoolExpression displayPreciseAnswerOnly, BoolExpression useTeamsAdaptiveCard = null)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var chatActivity = Activity.CreateMessageActivity();
            chatActivity.Text = result.Answer;

            List<CardAction> buttonList = null;
            if (result?.Context?.Prompts != null &&
                result.Context.Prompts.Any())
            {
                buttonList = new List<CardAction>();

                // Add all prompt
                foreach (var prompt in result.Context.Prompts)
                {
                    buttonList.Add(
                        new CardAction()
                        {
                            Value = prompt.QnaId,
                            Type = "messageBack",
                            Title = prompt.DisplayText,
                            Text = prompt.DisplayText,
                            DisplayText = prompt.DisplayText,
                        });
                }
            }

            string cardText = null;
            if (!string.IsNullOrWhiteSpace(result?.AnswerSpan?.Text))
            {
                chatActivity.Text = result.AnswerSpan.Text;

                // For content choice Precise only
                if (displayPreciseAnswerOnly.Value == false)
                {
                    cardText = result.Answer;
                }
            }

            if (buttonList != null || !string.IsNullOrWhiteSpace(cardText))
            {
                bool useAdaptive = useTeamsAdaptiveCard == null ? false : useTeamsAdaptiveCard.Value;
                var cardAttachment = useAdaptive ? GetAdaptiveCardAttachment(cardText, buttonList) : GetHeroCardAttachment(cardText, buttonList);

                chatActivity.Attachments.Add(cardAttachment);
            }

            return chatActivity;
        }

        /// <summary>
        /// Get a Teams-formatted Adaptive Card as Attachment to be returned in the QnA response.
        /// </summary>
        /// <param name="cardText">string of text to be added to the card.</param>
        /// <param name="buttonList">List of CardAction representing buttons to be added to the card.</param>
        /// <returns>Attachment.</returns>
        private static Attachment GetAdaptiveCardAttachment(string cardText, List<CardAction> buttonList)
        {
            // Create a list of buttons. Each button is represented by a Dictionary containing the required adaptive card fields
            var cardButtons = new List<Dictionary<string, object>>();

            if (buttonList != null)
            {
                foreach (var button in buttonList)
                {
                    // Create the initial dictionary
                    var adaptiveAction = new Dictionary<string, object>
                    {
                        { "type", "Action.Submit" },
                        { "title", button.Title }
                    };

                    // Create the "data" Dictionary, and add to it a Dictionary representing the "msteams" object
                    var data = new Dictionary<string, object>
                    {
                        {
                            "msteams",
                            new Dictionary<string, object>
                            {
                                { "type", "messageBack" },
                                { "displayText", button.DisplayText },
                                { "text", button.Text },
                                { "value", button.Value },
                                { "width", "full" }
                            }
                        }
                    };

                    // Add the data dictionary to the cardAction
                    adaptiveAction.Add("data", data);

                    // Add to the list of buttons
                    cardButtons.Add(adaptiveAction);
                }
            }

            // Create a dictionary to represent the completed Adaptive card
            // msteams field is also a dictionary
            // body field is an array containing a dictionary
            var card = new Dictionary<string, object>
            {
                { "$schema", "http://adaptivecards.io/schemas/adaptive-card.json" },
                { "type", "AdaptiveCard" },
                { "version", "1.3" },
                {
                   "msteams",
                   new Dictionary<string, string>
                   {
                       { "width", "full" },
                       { "height", "full" }
                   }
                },
                { 
                    "body",
                    new Dictionary<string, string>[]
                    {
                        new Dictionary<string, string>
                        {
                            { "type", "TextBlock" },
                            { "text", (!string.IsNullOrWhiteSpace(cardText) ? cardText : string.Empty) }
                        }
                    }
                }
            };

            // If there are buttons, add the buttons to the card as an array
            if (buttonList != null)
            {
                card.Add("actions", cardButtons.ToArray());
            }

            // Create and return the card as an attachment
            Attachment adaptiveCard = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card
            };

            return adaptiveCard;
        }

        /// <summary>
        /// Get a Hero Card as Attachment to be returned in the QnA response.
        /// </summary>
        /// <param name="cardText">string of text to be added to the card.</param>
        /// <param name="buttonList">List of CardAction representing buttons to be added to the card.</param>
        /// <returns>Attachment.</returns>
        private static Attachment GetHeroCardAttachment(string cardText, List<CardAction> buttonList) 
        {
            // Create a new hero card, add the text and buttons if they exist
            var card = new HeroCard();

            if (buttonList != null) 
            {
                card.Buttons = buttonList;
            }

            if (!string.IsNullOrWhiteSpace(cardText))
            {
                card.Text = cardText;
            }

            // Return the card as an attachment
            return card.ToAttachment();
        }
    }
}
