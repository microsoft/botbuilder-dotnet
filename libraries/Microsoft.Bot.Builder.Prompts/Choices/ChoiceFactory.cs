// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class ChoiceFactory
    {
        public static IMessageActivity ForChannel(ITurnContext context, IEnumerable<Choice> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return ForChannel(Channel.GetChannelId(context), choices, text, speak, options);
        }

        public static IMessageActivity ForChannel(ITurnContext context, IEnumerable<string> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return ForChannel(Channel.GetChannelId(context), ToChoices(choices), text, speak, options);
        }

        public static IMessageActivity ForChannel(string channelId, IEnumerable<string> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return ForChannel(channelId, ToChoices(choices), text, speak, options);
        }

        public static IMessageActivity ForChannel(string channelId, IEnumerable<Choice> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            var list = choices?.ToList() ?? new List<Choice>();

            // Find maximum title length
            var maxTitleLength = 0;
            foreach (var choice in list)
            {
                var l = choice.Action != null && string.IsNullOrEmpty(choice.Action.Title) ? choice.Action.Title.Length : choice.Value.Length;
                if (l > maxTitleLength)
                {
                    maxTitleLength = l;
                }
            };

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

        public static Activity Inline(IEnumerable<string> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return Inline(ToChoices(choices), text, speak, options);
        }

        public static Activity Inline(IEnumerable<Choice> choiceList, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            var choices = choiceList?.ToList() ?? new List<Choice>();
            options = options ?? new ChoiceFactoryOptions();

            var opt = new ChoiceFactoryOptions
            {
                InlineSeparator = options.InlineSeparator ?? ", ",
                InlineOr = options.InlineOr ?? " or ",
                InlineOrMore = options.InlineOrMore ?? ", or ",
                IncludeNumbers = options.IncludeNumbers ?? true
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
            txt += "";

            // Return activity with choices as an inline list.
            return MessageFactory.Text(txt, speak, InputHints.ExpectingInput);
        }

        public static Activity List(IEnumerable<string> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return List(ToChoices(choices), text, speak, options);
        }

        public static Activity List(IEnumerable<Choice> choiceList, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            var choices = choiceList?.ToList() ?? new List<Choice>();
            options = options ?? new ChoiceFactoryOptions();

            bool includeNumbers = options.IncludeNumbers ?? true;

            // Format list of choices
            var connector = string.Empty;
            var txt = (text ?? string.Empty);
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

        public static IMessageActivity SuggestedAction(IEnumerable<string> choices, string text = null, string speak = null)
        {
            return SuggestedAction(ToChoices(choices), text, speak);
        }

        public static IMessageActivity SuggestedAction(IEnumerable<Choice> choiceList, string text = null, string speak = null)
        {
            var choices = choiceList?.ToList() ?? new List<Choice>();

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
                        Title = choice.Value
                    };
                }
            }).ToList();

            // Return activity with choices as suggested actions
            return MessageFactory.SuggestedActions(actions, text, speak, InputHints.ExpectingInput);
        }

        public static List<Choice> ToChoices(IEnumerable<string> choices)
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