using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Prompts
{
    public interface IChoiceFactoryOptions
    {
        string InlineSeparator { get; }
        string InlineOr { get; }
        string InlineOrMore { get; }
    }

    public class DefaultChoiceFactoryOptions : IChoiceFactoryOptions
    {
        public string InlineSeparator => ", ";
        public string InlineOr => " or ";
        public string InlineOrMore => ", or ";
    }

    public static class ChoiceEx
    {
        public static IDictionary<IEnumerable<string>, string> ToDictionary(this IEnumerable<Choice> source)
        {
            var result = new Dictionary<IEnumerable<string>, string>();
            foreach (var item in source)
            {
                var key = item.Synonyms ?? new string[] { item.Value };
                result.Add(key, item.Value);
            }

            return result;
        }

        public static IList<Choice> ToChoices(this IEnumerable<string> choices)
        {
            return choices.Select(choice => new Choice() { Value = choice }).ToList();
        }

        public static IMessageActivity ChoicesForChannel(this IEnumerable<Choice> choices, ITurnContext context, string text, string speak, IChoiceFactoryOptions options = null, bool includeNumbers = true)
        {
            return choices.ChoicesForChannel(ChannelUtility.GetChannelId(context), text, speak, options, includeNumbers);
        }

        public static IMessageActivity ChoicesForChannel(this IEnumerable<Choice> choices, string channel, string text, string speak, IChoiceFactoryOptions options = null, bool includeNumbers = true)
        {
            // Normalize choices
            var values = choices.Select(choice => $"{(choice.Action?.Title ?? choice.Value)}");

            // Find maximum title length
            var maxTitleLength = values.Max(value => value.Length);

            // Determine list style
            var choicesLength = values.Count();
            var supportsSuggestedActions = ChannelUtility.SupportsSuggestedActions(channel, choicesLength);
            var supportsCardActions = ChannelUtility.SupportsCardActions(channel, choicesLength);
            var maxActionTitleLength = ChannelUtility.MaxActionTitleLength(channel);
            var hasMessageFeed = ChannelUtility.HasMessageFeed(channel);
            var longTitles = maxTitleLength > maxActionTitleLength;

            if (!longTitles && (supportsSuggestedActions || (!hasMessageFeed && supportsCardActions)))
            {
                // We always prefer showing choices using suggested actions. If the titles are too long, however,
                // we'll have to show them as a text list.
                return choices.SuggestedAction(text, speak);
            }
            else if (!longTitles && choicesLength <= 3)
            {
                // If the titles are short and there are 3 or less choices we'll use an inline list.
                return choices.ChoicesToInline(text, speak, options, includeNumbers);
            }

            // Show a numbered list.
            return choices.ChoicesToList(text, speak, includeNumbers);
        }

        public static IMessageActivity ChoicesToInline(this IEnumerable<Choice> choices, string text, string speak, IChoiceFactoryOptions options = null, bool includeNumbers = true)
        {
            var opt = options ?? new DefaultChoiceFactoryOptions();
            
            // Format list of choices
            var values = choices.Select((choice, index) => $"{(includeNumbers ? $"{index + 1}. " : "- ")}{(choice.Action?.Title ?? choice.Value)}");
            var txt = new StringBuilder(text ?? "").Append(" ");
            if (values.Count() > 2)
            {
                txt = txt.Append(string.Join(opt.InlineSeparator, values.Take(values.Count() - 1)))
                    .Append(opt.InlineOrMore)
                    .Append(values.Last());
            }
            else
            {
                txt = txt.Append(string.Join(opt.InlineOr, values));
            }

            // Return activity with choices as an inline list.
            return MessageFactory.Text(txt.ToString(), speak, InputHints.ExpectingInput);
        }

        public static IMessageActivity ChoicesToList(this IEnumerable<Choice> choices, string text, string speak, bool includeNumbers = true)
        {
            // Format list of choices
            var values = choices.Select((choice, index) => $"{(includeNumbers ? $"{index + 1}. " : "- ")}{(choice.Action?.Title ?? choice.Value)}");
            var txt = new StringBuilder(text ?? "")
                .AppendLine()
                .AppendLine()
                .Append(string.Join(Environment.NewLine, values))
                .ToString();

            // Return activity with choices as a numbered list.
            return MessageFactory.Text(txt, speak, InputHints.ExpectingInput);
        }

        public static IMessageActivity SuggestedAction(this IEnumerable<Choice> choices, string text, string speak)
        {
            // Map choices to actions
            var actions = choices.Select(choice => {
                return choice.Action ?? new CardAction()
                {
                    Type = ActionTypes.ImBack,
                    Value = choice.Value,
                    Title = choice.Value
                };
            }).ToList();

            // Return activity with choices as suggested actions
            return MessageFactory.SuggestedActions(actions, text, speak, InputHints.ExpectingInput);
        }
    }
}
