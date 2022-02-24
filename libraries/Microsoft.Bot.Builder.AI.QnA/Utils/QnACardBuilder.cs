// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Get Card for MultiTurn scenario. (Can be deprected from 4.10.0 release of sdk).
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
        /// <returns>IMessageActivity.</returns>
        public static IMessageActivity GetQnADefaultResponse(QueryResult result, bool displayPreciseAnswerOnly)
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
                            Value = prompt.DisplayText,
                            Type = "imBack",
                            Title = prompt.DisplayText,
                        });
                }
            }

            string heroCardText = null;
            if (!string.IsNullOrWhiteSpace(result?.AnswerSpan?.Text))
            {
                chatActivity.Text = result.AnswerSpan.Text;

                // For content choice Precise only
                if (!displayPreciseAnswerOnly)
                {
                    heroCardText = result.Answer;
                }
            }

            if (buttonList != null || !string.IsNullOrWhiteSpace(heroCardText))
            {
                var plCard = new HeroCard();

                if (buttonList != null)
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
