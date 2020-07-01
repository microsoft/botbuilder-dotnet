// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Assists with formatting a message activity that contains a list of choices.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class ChoiceFactory
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Creates a message activity that includes a list of choices formatted based on the capabilities of a given channel.
        /// </summary>
        /// <param name="channelId">A channel ID. The <see cref="Connector.Channels"/> class contains known channel IDs.</param>
        /// <param name="list">The list of choices to include.</param>
        /// <param name="text">Optional, the text of the message to send.</param>
        /// <param name="speak">Optional, the text to be spoken by your bot on a speech-enabled channel.</param>
        /// <param name="options">Optional, the formatting options to use when rendering as a list.</param>
        /// <returns>The created message activity.</returns>
        /// <remarks>The algorithm prefers to format the supplied list of choices as suggested actions but can decide
        /// to use a text based list if suggested actions aren't natively supported by the channel, there are too many
        /// choices for the channel to display, or the title of any choice is too long.
        /// <para>If the algorithm decides to use a list, for 3 or fewer choices with short titles it will use an inline
        /// list; otherwise, a numbered list.</para></remarks>
        public static IMessageActivity ForChannel(string channelId, IList<Choice> list, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            channelId ??= string.Empty;
            list ??= new List<Choice>();

            // Find maximum title length
            var maxTitleLength = 0;
            foreach (var choice in list)
            {
                var l = choice.Action != null && !string.IsNullOrEmpty(choice.Action.Title) ? choice.Action.Title.Length : choice.Value.Length;
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

            if (!longTitles && !supportsSuggestedActions && supportsCardActions)
            {
                // SuggestedActions is the preferred approach, but for channels that don't
                // support them (e.g. Teams, Cortana) we should use a HeroCard with CardActions
                return HeroCard(list, text, speak);
            }

            if (!longTitles && supportsSuggestedActions)
            {
                // We always prefer showing choices using suggested actions. If the titles are too long, however,
                // we'll have to show them as a text list.
                return SuggestedAction(list, text, speak);
            }

            if (!longTitles && list.Count <= 3)
            {
                // If the titles are short and there are 3 or less choices we'll use an inline list.
                return Inline(list, text, speak, options);
            }

            // Show a numbered list.
            return List(list, text, speak, options);
        }

        /// <summary>
        /// Creates a message activity that includes a list of choices formatted as an inline list.
        /// </summary>
        /// <param name="choices">The list of choices to include.</param>
        /// <param name="text">Optional, the text of the message to send.</param>
        /// <param name="speak">Optional, the text to be spoken by your bot on a speech-enabled channel.</param>
        /// <param name="options">Optional, the formatting options to use when rendering as a list.</param>
        /// <returns>The created message activity.</returns>
        public static Activity Inline(IList<Choice> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            choices ??= new List<Choice>();
            options ??= new ChoiceFactoryOptions();

            var opt = new ChoiceFactoryOptions
            {
                InlineSeparator = options.InlineSeparator ?? ", ",
                InlineOr = options.InlineOr ?? " or ",
                InlineOrMore = options.InlineOrMore ?? ", or ",
                IncludeNumbers = options.IncludeNumbers ?? true,
            };

            // Format list of choices
            var connector = string.Empty;
            var txtBuilder = new StringBuilder(text)
                .Append(' ');
            for (var index = 0; index < choices.Count; index++)
            {
                var choice = choices[index];
                var title = choice.Action != null && choice.Action.Title != null ? choice.Action.Title : choice.Value;

                txtBuilder.Append(connector);
                if (opt.IncludeNumbers.Value)
                {
                    txtBuilder
                        .Append('(')
                        .Append(index + 1)
                        .Append(") ");
                }

                txtBuilder.Append(title);
                if (index == choices.Count - 2)
                {
                    connector = (index == 0 ? opt.InlineOr : opt.InlineOrMore) ?? string.Empty;
                }
                else
                {
                    connector = opt.InlineSeparator ?? string.Empty;
                }
            }

            // Return activity with choices as an inline list.
            return MessageFactory.Text(txtBuilder.ToString(), speak, InputHints.ExpectingInput);
        }

        /// <summary>
        /// Creates a message activity that includes a list of choices formatted as a numbered or bulleted list.
        /// </summary>
        /// <param name="choices">The list of choices to include.</param>
        /// <param name="text">Optional, the text of the message to send.</param>
        /// <param name="speak">Optional, the text to be spoken by your bot on a speech-enabled channel.</param>
        /// <param name="options">Optional, the formatting options to use when rendering as a list.</param>
        /// <returns>The created message activity.</returns>
        public static Activity List(IList<string> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            return List(ToChoices(choices), text, speak, options);
        }

        public static Activity List(IList<Choice> choices, string text = null, string speak = null, ChoiceFactoryOptions options = null)
        {
            choices ??= new List<Choice>();
            options ??= new ChoiceFactoryOptions();

            var includeNumbers = options.IncludeNumbers ?? true;

            // Format list of choices
            var connector = string.Empty;
            var txtBuilder = new StringBuilder(text)
                .Append("\n\n   ");

            for (var index = 0; index < choices.Count; index++)
            {
                var choice = choices[index];

                var title = choice.Action?.Title ?? choice.Value;

                txtBuilder.Append(connector);
                if (includeNumbers)
                {
                    txtBuilder
                        .Append(index + 1)
                        .Append(". ");
                }
                else
                {
                    txtBuilder.Append("- ");
                }

                txtBuilder.Append(title);
                connector = "\n   ";
            }

            // Return activity with choices as a numbered list.
            return MessageFactory.Text(txtBuilder.ToString(), speak, InputHints.ExpectingInput);
        }

        public static IMessageActivity SuggestedAction(IList<string> choices, string text = null, string speak = null)
        {
            return SuggestedAction(ToChoices(choices), text, speak);
        }

        public static IMessageActivity SuggestedAction(IList<Choice> choices, string text = null, string speak = null)
        {
            // Return activity with choices as suggested actions
            return MessageFactory.SuggestedActions(ExtractActions(choices), text, speak, InputHints.ExpectingInput);
        }

        public static IMessageActivity HeroCard(IList<Choice> choices, string text = null, string speak = null)
        {
            var attachments = new List<Attachment>
            {
                new HeroCard(text: text, buttons: ExtractActions(choices)).ToAttachment(),
            };

            // Return activity with choices as HeroCard with buttons
            return MessageFactory.Attachment(attachments, null, speak, InputHints.ExpectingInput);
        }

        public static IList<Choice> ToChoices(IList<string> choices)
        {
            return choices == null
                ? new List<Choice>()
                : choices.Select(choice => new Choice { Value = choice }).ToList();
        }

        private static List<CardAction> ExtractActions(IList<Choice> choices)
        {
            choices ??= new List<Choice>();

            // Map choices to actions
            return choices.Select(choice =>
            {
                if (choice.Action != null)
                {
                    return choice.Action;
                }

                return new CardAction
                {
                    Type = ActionTypes.ImBack,
                    Value = choice.Value,
                    Title = choice.Value,
                };
            }).ToList();
        }
    }
}
