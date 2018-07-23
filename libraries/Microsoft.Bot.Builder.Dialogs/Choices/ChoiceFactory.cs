// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    public class ChoiceFactory
    {
        public static IMessageActivity ForChannel(string channelId, List<Choice> list, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            channelId = channelId ?? string.Empty;

            list = list ?? new List<Choice>();

            // Find maximum title length
            var maxTitleLength = 0;
            foreach (var choice in list)
            {
                var l = choice.Action != null && string.IsNullOrEmpty(choice.Action.Title) ? choice.Action.Title.Length : choice.Value.Length;
                if (l > maxTitleLength)
                {
                    maxTitleLength = l;
                }
            }

            // Determine list style
            var supportsSuggestedActions = Channel.SupportsSuggestedActions(channelId, list.Count);
            var supportsCardActions = Channel.SupportsCardActions(channelId, list.Count);
            var maxActionTitleLength = Channel.MaxActionTitleLength(channelId);
            var hasMessageFeed = Channel.HasMessageFeed(channelId);
            var longTitles = maxTitleLength > maxActionTitleLength;

            if (!longTitles && (supportsSuggestedActions || (!hasMessageFeed && supportsCardActions)))
            {
                // We always prefer showing choices using suggested actions. If the titles are too long, however,
                // we'll have to show them as a text list.
                return SuggestedAction(list, text, speak);
            }
            else if (!longTitles && list.Count <= 3)
            {
                // If the titles are short and there are 3 or less choices we'll use an inline list.
                return Inline(list, text, speak, options);
            }
            else
            {
                // Show a numbered list.
                return List(list, text, speak, options);
            }
        }

        public static Activity Inline(List<Choice> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            choices = choices ?? new List<Choice>();
            options = options ?? new ChoiceFactoryOptions();

            var opt = new ChoiceFactoryOptions
            {
                InlineSeparator = options.InlineSeparator ?? ", ",
                InlineOr = options.InlineOr ?? " or ",
                InlineOrMore = options.InlineOrMore ?? ", or ",
                IncludeNumbers = options.IncludeNumbers ?? true,
            };

            // Format list of choices
            var connector = string.Empty;
            var txt = text ?? string.Empty;
            txt += " ";

            for (var index = 0; index < choices.Count; index++)
            {
                var choice = choices[index];

                var title = choice.Action != null && choice.Action.Title != null ? choice.Action.Title : choice.Value;

                txt += $"{connector}";
                if (opt.IncludeNumbers.Value)
                {
                    txt += "(" + (index + 1).ToString() + ") ";
                }

                txt += $"{title}";
                if (index == (choices.Count - 2))
                {
                    connector = (index == 0 ? opt.InlineOr : opt.InlineOrMore) ?? string.Empty;
                }
                else
                {
                    connector = opt.InlineSeparator ?? string.Empty;
                }
            }

            txt += string.Empty;

            // Return activity with choices as an inline list.
            return MessageFactory.Text(txt, speak, InputHints.ExpectingInput);
        }

        public static Activity List(List<string> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return List(ToChoices(choices), text, speak, options);
        }

        public static Activity List(List<Choice> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            choices = choices ?? new List<Choice>();
            options = options ?? new ChoiceFactoryOptions();

            var includeNumbers = options.IncludeNumbers ?? true;

            // Format list of choices
            var connector = string.Empty;
            var txt = text ?? string.Empty;
            txt += "\n\n   ";

            for (var index = 0; index < choices.Count; index++)
            {
                var choice = choices[index];

                var title = choice.Action != null && choice.Action.Title != null ? choice.Action.Title : choice.Value;

                txt += connector;
                if (includeNumbers)
                {
                    txt += (index + 1).ToString() + ". ";
                }
                else
                {
                    txt += "- ";
                }

                txt += title;
                connector = "\n   ";
            }

            // Return activity with choices as a numbered list.
            return MessageFactory.Text(txt, speak, InputHints.ExpectingInput);
        }

        public static IMessageActivity SuggestedAction(List<string> choices, string text = null, string speak = null)
        {
            return SuggestedAction(ToChoices(choices), text, speak);
        }

        public static IMessageActivity SuggestedAction(List<Choice> choices, string text = null, string speak = null)
        {
            choices = choices ?? new List<Choice>();

            // Map choices to actions
            var actions = choices.Select((choice) =>
            {
                if (choice.Action != null)
                {
                    return choice.Action;
                }
                else
                {
                    return new CardAction
                    {
                        Type = ActionTypes.ImBack,
                        Value = choice.Value,
                        Title = choice.Value,
                    };
                }
            }).ToList();

            // Return activity with choices as suggested actions
            return MessageFactory.SuggestedActions(actions, text, speak, InputHints.ExpectingInput);
        }

        public static List<Choice> ToChoices(List<string> choices)
        {
            return (choices == null)
                    ?
                new List<Choice>()
                    :
                choices.Select(choice => new Choice { Value = choice }).ToList();
        }

        public static List<Choice> ToChoicesList(Tuple<Choice, Choice> choices)
        {
            return (choices == null)
                    ?
                new List<Choice>()
                    :
                new List<Choice> { choices.Item1, choices.Item2 };
        }
    }
}
