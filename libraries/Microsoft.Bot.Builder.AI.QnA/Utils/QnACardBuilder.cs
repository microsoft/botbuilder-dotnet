// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// <param name="result">Result to be displayed.</param>
        /// <param name="cardNoMatchText">No match text.</param>
        /// <returns>IMessageActivity.</returns>
        public static IMessageActivity GetQnAPromptsCard(QueryResult result, string cardNoMatchText)
        {
            return GetQnADefaultResponse(result, true);
        }

        /// <summary>
        /// Get Answer Card.
        /// </summary>
        /// <param name="result">Result to be displayed.</param>
        /// <param name="displayPreciseAnswerOnly">Choice whether to display Precise Answer Only or source text along with Precise Answer.</param>  
        /// <returns>Message activity for Query Result.</returns>
        public static IMessageActivity GetQnADefaultResponse(QueryResult result, bool displayPreciseAnswerOnly)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var chatActivity = Activity.CreateMessageActivity();
            chatActivity.Text = result.Answer;
            
            var buttonList = new List<CardAction>();
            if (result?.Context?.Prompts != null &&
                result.Context.Prompts.Any())
            {
                // Add all prompts
                foreach (var prompt in result.Context.Prompts)
                {
                    buttonList.Add(
                        new CardAction()
                        {
                            Value = prompt.DisplayText,
                            Type = "imBack",
                            Title = prompt.DisplayText,
                        });
                }
            }

            string heroCardText = string.Empty;
            if (!string.IsNullOrWhiteSpace(result?.AnswerSpan?.Text))
            {
                chatActivity.Text = result.AnswerSpan.Text;

                // When the configuration is set to display precise answer only
                if (!displayPreciseAnswerOnly)
                {
                    heroCardText = result.Answer;
                }
            }

            if (buttonList.Any() || !string.IsNullOrWhiteSpace(heroCardText))
            {
                var plCard = new HeroCard();  

                if (buttonList.Any())
                {
                    plCard.Buttons = buttonList;
                }

                if (!string.IsNullOrWhiteSpace(heroCardText))
                {
                    plCard.Text = heroCardText;
                }

                // Create the attachment.
                var attachment = plCard.ToAttachment();

                chatActivity.Attachments.Add(attachment);
            }

            return chatActivity;
        }
    }
}
