// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Choices
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
            if (choices is null)
            {
                throw new ArgumentNullException("The collection of choices can not be null.", nameof(choices));
            }

            var list = new List<Choice>();

            // Find maximum title length
            var maxTitleLength = 0;
            foreach (var choice in choices)
            {
                int len = GetTitle(choice).Length;
                if (len > maxTitleLength)
                {
                    maxTitleLength = len;
                }

                list.Add(choice);
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

        public static Activity Inline(IEnumerable<string> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return Inline(ToChoices(choices), text, speak, options);
        }

        public static Activity Inline(IEnumerable<Choice> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            if (choices is null)
            {
                throw new ArgumentNullException("The collection of choices can not be null.", nameof(choices));
            }

            options = options ?? new ChoiceFactoryOptions();

            var opt = new ChoiceFactoryOptions
            {
                InlineSeparator = options.InlineSeparator ?? ", ",
                InlineOr = options.InlineOr ?? " or ",
                InlineOrMore = options.InlineOrMore ?? ", or ",
                IncludeNumbers = options.IncludeNumbers ?? true
            };

            // Format list of choices
            var separator = string.Empty;
            var sb = new StringBuilder();
            if (text != null)
            {
                sb.Append(text);
            }
            sb.Append(' ');

            var list = choices.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var choice = list[i];
                if (choice is null)
                {
                    throw new ArgumentException("Set of choices contained a null element, which is not allowed.", nameof(choices));
                }

                string title = GetTitle(choice);
                sb.Append(separator);
                if (opt.IncludeNumbers.Value)
                {
                    sb.Append('(');
                    sb.Append(i + 1);
                    sb.Append(") ");
                }
                sb.Append(title);

                // Insert separators between items.
                // Use an "Oxford comma"-style separator for three or more items.
                separator = (i == list.Count - 2)
                    ? (i == 0 ? opt.InlineOr : opt.InlineOrMore) ?? string.Empty
                    : opt.InlineSeparator ?? string.Empty;
            }

            // Return activity with choices as an inline list.
            return MessageFactory.Text(sb.ToString(), speak, InputHints.ExpectingInput);
        }

        /// <summary>
        /// Gets a "normalized" title for a <see cref="Choice"/> object.
        /// </summary>
        /// <param name="choice">The choice object.</param>
        /// <returns>The normalized title.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException">The choice does not have a valid action title or value to use as a title.</exception>
        private static string GetTitle(Choice choice)
        {
            if (choice is null) throw new ArgumentException();

            if (!string.IsNullOrWhiteSpace(choice.Action?.Title)) return choice.Action.Title.Trim();
            if (!string.IsNullOrWhiteSpace(choice.Value)) return choice.Value.Trim();

            throw new InvalidOperationException("Each choice must specify either an action title or a value.");
        }

        public static Activity List(IEnumerable<string> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return List(ToChoices(choices), text, speak, options);
        }

        public static Activity List(IEnumerable<Choice> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            options = options ?? new ChoiceFactoryOptions();

            bool includeNumbers = options.IncludeNumbers ?? true;

            // Format list of choices
            var separator = string.Empty;
            var sb = new StringBuilder();
            if (text != null)
            {
                sb.Append(text);
            }
            sb.Append("\n\n   ");

            int index = 1;
            foreach (var choice in choices)
            {
                if (choice is null)
                {
                    throw new ArgumentException("Set of choices contained a null element, which is not allowed.", nameof(choices));
                }

                string title = GetTitle(choice);
                sb.Append(separator);
                if (includeNumbers)
                {
                    sb.Append(index);
                    sb.Append(". ");
                }
                else
                {
                    sb.Append("- ");
                }
                sb.Append(title);

                separator = "\n   ";
                index++;
            }

            // Return activity with choices as a numbered list.
            return MessageFactory.Text(sb.ToString(), speak, InputHints.ExpectingInput);
        }

        public static IMessageActivity SuggestedAction(IEnumerable<string> choices, string text = null, string speak = null)
        {
            return SuggestedAction(ToChoices(choices), text, speak);
        }

        public static IMessageActivity SuggestedAction(IEnumerable<Choice> choices, string text = null, string speak = null)
        {
            var choiceList = choices?.ToList() ?? new List<Choice>();

            // Map choices to actions
            var actions = choiceList.Select((choice) =>
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