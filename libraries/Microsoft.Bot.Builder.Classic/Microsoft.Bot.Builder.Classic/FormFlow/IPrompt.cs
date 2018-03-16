// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Resource;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{

    /// <summary>
    /// Interface for a prompt and its associated recognizer.
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    /// <remarks>
    /// This interface allows taking a \ref patterns expression and making it into a string with the template parts filled in.
    /// </remarks>
    public interface IPrompt<T>
        where T : class
    {
        /// <summary>
        /// Description of the prompt and how to generate it.
        /// </summary>
        /// <returns>Attribute describing how to generate prompt.</returns>
        TemplateBaseAttribute Annotation { get; }

        /// <summary>
        /// Return prompt to send to user.
        /// </summary>
        /// <param name="state">Current form state.</param>
        /// <param name="field">Current field being processed.</param>
        /// <param name="args">Optional arguments.</param>
        /// <returns>Message to user.</returns>
        FormPrompt Prompt(T state, IField<T> field, params object[] args);

        /// <summary>
        /// Associated recognizer if any.
        /// </summary>
        /// <returns>Recognizer for matching user input.</returns>
        IRecognize<T> Recognizer { get; }
    }

    /// <summary>
    /// The prompt that is returned by form prompter. 
    /// </summary>
    [Serializable]
    public sealed class FormPrompt : ICloneable
    {
        /// <summary>
        /// The text prompt that corresponds to Message.Text.
        /// </summary>
        /// <remarks>When generating cards this will be the card title.</remarks>
        public string Prompt { set; get; } = string.Empty;

        /// <summary>
        /// Description information for generating cards.
        /// </summary>
        public DescribeAttribute Description { set; get; }

        /// <summary>
        /// The buttons that will be mapped to Message.Attachments.
        /// </summary>
        public IList<DescribeAttribute> Buttons { set; get; } = new List<DescribeAttribute>();

        /// <summary>
        /// Desired prompt style.
        /// </summary>
        public ChoiceStyleOptions Style;

        public override string ToString()
        {
            return $"{Prompt} {Language.BuildList(Buttons.Select(button => button.ToString()), Resources.DefaultChoiceSeparator, Resources.DefaultChoiceLastSeparator)}";
        }

        /// <summary>
        /// Deep clone the FormPrompt.
        /// </summary>
        /// <returns> A deep cloned instance of FormPrompt.</returns>
        public object Clone()
        {
            var newPrompt = new FormPrompt();
            newPrompt.Prompt = this.Prompt;
            newPrompt.Description = this.Description;
            newPrompt.Buttons = new List<DescribeAttribute>(this.Buttons);
            newPrompt.Style = this.Style;
            return newPrompt;
        }
    }

    /// <summary>
    /// A Form button that will be mapped to Connector.Action.
    /// </summary>
    [Serializable]
    public sealed class FormButton : ICloneable
    {
        /// <summary>
        /// Picture which will appear on the button.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Message that will be sent to bot when this button is clicked.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Label of the button.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL which will be opened in the browser built-into Client application.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Clone the FormButton
        /// </summary>
        /// <returns> A new cloned instance of object.</returns>
        public object Clone()
        {
            return new FormButton
            {
                Image = this.Image,
                Message = this.Message,
                Title = this.Title,
                Url = this.Url
            };
        }

        /// <summary>
        /// ToString() override. 
        /// </summary>
        /// <returns> Title of the button.</returns>
        public override string ToString()
        {
            return Title;
        }
    }

    /// <summary>
    /// A delegate for styling and posting a prompt.
    /// </summary>
    /// <param name="context">Message context.</param>
    /// <param name="prompt">Prompt to be posted.</param>
    /// <param name="state">State of the form.</param>
    /// <param name="field">Field being prompted or null if not a field.</param>
    /// <returns>Prompt that was posted.</returns>
    public delegate Task<FormPrompt> PromptAsyncDelegate<T>(IDialogContext context, FormPrompt prompt, T state, IField<T> field)
        where T : class;

    public static partial class Extensions
    {
        /// <summary>
        /// Generate a hero card from a FormPrompt.
        /// </summary>
        /// <param name="prompt">Prompt definition.</param>
        /// <returns>Either an empty list if no buttons or a list with one hero card.</returns>
        public static IList<Attachment> GenerateHeroCard(this FormPrompt prompt)
        {
            var actions = new List<CardAction>();
            foreach (var button in prompt.Buttons)
            {
                actions.Add(new CardAction(ActionTypes.ImBack, button.Description, button.Image, button.Message ?? button.Description));
            }

            var attachments = new List<Attachment>();
            if (actions.Count > 0)
            {
                var description = prompt.Description;
                // Facebook requires a title https://github.com/Microsoft/BotBuilder/issues/1678
                attachments.Add(new HeroCard(text: prompt.Prompt, title: description.Title ?? string.Empty, subtitle: description.SubTitle,
                    buttons: actions,
                    images: prompt.Description?.Image == null ? null : new List<CardImage>() { new CardImage() { Url = description.Image } })
                    .ToAttachment());
            }
            return attachments;
        }

        /// <summary>
        /// Generate a list of hero cards from a prompt definition.
        /// </summary>
        /// <param name="prompt">Prompt definition.</param>
        /// <returns>List of hero cards.</returns>
        public static IList<Attachment> GenerateHeroCards(this FormPrompt prompt)
        {
            var attachments = new List<Attachment>();
            var description = prompt.Description;
            foreach (var button in prompt.Buttons)
            {
                string image = button.Image ?? description.Image;
                attachments.Add(new HeroCard(
                    title: button.Title ?? description.Title ?? string.Empty,
                    subtitle: button.SubTitle ?? description.SubTitle,
                    text: prompt.Prompt,
                    images: (image == null ? null : (new List<CardImage>() { new CardImage() { Url = image } })),
                    buttons: new List<CardAction>() { new CardAction(ActionTypes.ImBack, button.Description, null, button.Message ?? button.Description) })
                    .ToAttachment());
            }
            return attachments;
        }

        /// <summary>
        /// Given a prompt definition generate messages to send back.
        /// </summary>
        /// <param name="prompt">Prompt definition.</param>
        /// <param name="preamble">Simple text message with all except last line of prompt to allow markdown in prompts.</param>
        /// <param name="promptMessage">Message with prompt definition including cards.</param>
        /// <returns>True if preamble should be sent.</returns>
        public static bool GenerateMessages(this FormPrompt prompt, IMessageActivity preamble, IMessageActivity promptMessage)
        {
            var promptCopy = (FormPrompt) prompt.Clone();
            bool hasPreamble = false;
            if (promptCopy.Buttons?.Count > 0 || promptCopy.Description?.Image != null)
            {
                // If we are generating cards we do not support markdown so create a separate message
                // for all lines except the last one.  
                var lines = promptCopy.Prompt.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if (lines.Length > 1)
                {
                    var builder = new StringBuilder();
                    for (var i = 0; i < lines.Length - 1; ++i)
                    {
                        if (i > 0)
                        {
                            builder.AppendLine();
                        }
                        builder.Append(lines[i]);
                    }
                    preamble.Text = builder.ToString();
                    promptCopy.Prompt = lines.Last();
                    hasPreamble = true;
                }
                if (promptCopy.Buttons?.Count > 0)
                {
                    var style = promptCopy.Style;
                    if (style == ChoiceStyleOptions.Auto)
                    {
                        foreach (var button in promptCopy.Buttons)
                        {
                            // Images require carousel
                            if (button.Image != null)
                            {
                                style = ChoiceStyleOptions.Carousel;
                                break;
                            }
                        }
                    }
                    if (style == ChoiceStyleOptions.Carousel)
                    {
                        promptMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        promptMessage.Attachments = promptCopy.GenerateHeroCards();
                    }
                    else
                    {
                        promptMessage.AttachmentLayout = AttachmentLayoutTypes.List;
                        promptMessage.Attachments = promptCopy.GenerateHeroCard();
                    }
                }
                else if (promptCopy.Description?.Image != null)
                {
                    promptMessage.AttachmentLayout = AttachmentLayoutTypes.List;
                    var card = new HeroCard() { Title = promptCopy.Prompt, Images = new List<CardImage> { new CardImage(promptCopy.Description.Image) } };
                    promptMessage.Attachments = new List<Attachment> { card.ToAttachment() };
                }
            }
            else
            {
                promptMessage.Text = promptCopy.Prompt;
            }
            return hasPreamble;
        }

        internal static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
        {
            foreach (var cur in enumerable)
            {
                collection.Add(cur);
            }
        }

        internal static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    #region Documentation
    /// <summary>   A prompt and recognizer packaged together. </summary>
    /// <typeparam name="T">    UNderlying form type. </typeparam>
    #endregion
    public sealed class Prompter<T> : IPrompt<T>
        where T : class
    {
        /// <summary>
        /// Construct a prompter.
        /// </summary>
        /// <param name="annotation">Annotation describing the \ref patterns and formatting for prompt.</param>
        /// <param name="form">Current form.</param>
        /// <param name="recognizer">Recognizer if any.</param>
        /// <param name="fields">Fields name lookup.  (Defaults to forms.)</param>
        public Prompter(TemplateBaseAttribute annotation, IForm<T> form, IRecognize<T> recognizer, IFields<T> fields = null)
        {
            annotation.ApplyDefaults(form.Configuration.DefaultPrompt);
            _annotation = annotation;
            _form = form;
            _fields = fields ?? form.Fields;
            _recognizer = recognizer;
        }

        public TemplateBaseAttribute Annotation
        {
            get
            {
                return _annotation;
            }
        }

        public FormPrompt Prompt(T state, IField<T> field, params object[] args)
        {
            string currentChoice = null;
            string noValue = null;
            if (field != null)
            {
                currentChoice = field.Template(TemplateUsage.CurrentChoice).Pattern();
                if (field.Optional)
                {
                    noValue = field.Template(TemplateUsage.NoPreference).Pattern();
                }
                else
                {
                    noValue = field.Template(TemplateUsage.Unspecified).Pattern();
                }
            }
            IList<DescribeAttribute> buttons = new List<DescribeAttribute>();
            var response = ExpandTemplate(_annotation.Pattern(), currentChoice, noValue, state, field, args, ref buttons);
            return new FormPrompt
            {
                Prompt = (response == null ? string.Empty : _spacesPunc.Replace(_spaces.Replace(Language.ANormalization(response), "$1 "), "$1")),
                Description = field?.FieldDescription,
                Buttons = buttons,
                Style = _annotation.ChoiceStyle
            };
        }

        public IRecognize<T> Recognizer
        {
            get { return _recognizer; }
        }

        #region Documentation
        /// <summary>   Validate pattern by ensuring they refer to real fields. </summary>
        /// <param name="form">     The form. </param>
        /// <param name="pattern">  Specifies the pattern. </param>
        /// <param name="field"> Base field for pattern. </param>
        /// <param name="argLimit"> The number of arguments passed to the pattern. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        #endregion
        public static bool ValidatePattern(IForm<T> form, string pattern, IField<T> field, int argLimit = 0)
        {
            bool ok = true;
            var fields = form.Fields;
            foreach (Match match in _args.Matches(pattern))
            {
                var expr = match.Groups[1].Value.Trim();
                int numeric;
                if (expr == "||")
                {
                    ok = true;
                }
                else if (expr.StartsWith("&"))
                {
                    var name = expr.Substring(1);
                    if (name == string.Empty && field != null) name = field.Name;
                    ok = (name == string.Empty || fields.Field(name) != null);
                }
                else if (expr.StartsWith("?"))
                {
                    ok = ValidatePattern(form, expr.Substring(1), field, argLimit);
                }
                else if (expr.StartsWith("["))
                {
                    if (expr.EndsWith("]"))
                    {
                        ok = ValidatePattern(form, expr.Substring(1, expr.Length - 2), field, argLimit);
                    }
                    else
                    {
                        ok = false;
                    }
                }
                else if (expr.StartsWith("*"))
                {
                    ok = (expr == "*" || expr == "*filled");
                }
                else if (TryParseFormat(expr, out numeric))
                {
                    ok = numeric <= argLimit - 1;
                }
                else
                {
                    var formatArgs = expr.Split(':');
                    var name = formatArgs[0];
                    if (name == string.Empty && field != null) name = field.Name;
                    ok = (name == string.Empty || fields.Field(name) != null);
                }
                if (!ok)
                {
                    break;
                }
            }
            return ok;
        }

        private string ExpandTemplate(string template, string currentChoice, string noValue, T state, IField<T> field, object[] args, ref IList<DescribeAttribute> buttons)
        {
            bool foundUnspecified = false;
            int last = 0;
            int numeric;
            var response = new StringBuilder();

            foreach (Match match in _args.Matches(template))
            {
                var expr = match.Groups[1].Value.Trim();
                var substitute = string.Empty;
                if (expr.StartsWith("&"))
                {
                    var name = expr.Substring(1);
                    if (name == string.Empty && field != null) name = field.Name;
                    var pathField = _fields.Field(name);
                    substitute = Language.Normalize(pathField == null ? field.Name : pathField.FieldDescription.Description, _annotation.FieldCase);
                }
                else if (expr == "||")
                {
                    var builder = new StringBuilder();
                    var values = _recognizer.ValueDescriptions();
                    var useButtons = !field.AllowsMultiple
                        && (_annotation.ChoiceStyle == ChoiceStyleOptions.Auto
                            || _annotation.ChoiceStyle == ChoiceStyleOptions.Buttons
                            || _annotation.ChoiceStyle == ChoiceStyleOptions.Carousel);
                    if (values.Any() && _annotation.AllowDefault != BoolDefault.False && field.Optional)
                    {
                        values = values.Concat(new DescribeAttribute[] { new DescribeAttribute(Language.Normalize(noValue, _annotation.ChoiceCase)) });
                    }
                    string current = null;
                    if (_annotation.AllowDefault != BoolDefault.False)
                    {
                        if (!field.Optional)
                        {
                            if (!field.IsUnknown(state))
                            {
                                current = ExpandTemplate(currentChoice, null, noValue, state, field, args, ref buttons);
                            }
                        }
                        else
                        {
                            current = ExpandTemplate(currentChoice, null, noValue, state, field, args, ref buttons);
                        }
                    }
                    if (values.Any())
                    {
                        if (useButtons)
                        {
                            foreach (var value in values)
                            {
                                buttons.Add(value);
                            }
                        }
                        else
                        {
                            // Buttons do not support multiple selection so we fall back to text
                            if (((_annotation.ChoiceStyle == ChoiceStyleOptions.Auto || _annotation.ChoiceStyle == ChoiceStyleOptions.AutoText)
                                && values.Count() < 4)
                                || (_annotation.ChoiceStyle == ChoiceStyleOptions.Inline))
                            {
                                // Inline choices
                                if (_annotation.ChoiceParens == BoolDefault.True) builder.Append('(');
                                var choices = new List<string>();
                                var i = 1;
                                foreach (var value in values)
                                {
                                    choices.Add(string.Format(_annotation.ChoiceFormat, i, Language.Normalize(value.Description, _annotation.ChoiceCase)));
                                    ++i;
                                }
                                builder.Append(Language.BuildList(choices, _annotation.ChoiceSeparator, _annotation.ChoiceLastSeparator));
                                if (_annotation.ChoiceParens == BoolDefault.True) builder.Append(')');
                                if (current != null)
                                {
                                    builder.Append(" ");
                                    builder.Append(current);
                                }
                            }
                            else
                            {
                                // Separate line choices
                                if (current != null)
                                {
                                    builder.Append(current);
                                    builder.Append(" ");
                                }
                                var i = 1;
                                foreach (var value in values)
                                {
                                    builder.AppendLine();
                                    builder.Append("  ");
                                    if (!_annotation.AllowNumbers)
                                    {
                                        builder.Append("* ");
                                    }
                                    builder.AppendFormat(_annotation.ChoiceFormat, i, Language.Normalize(value.Description, _annotation.ChoiceCase));
                                    ++i;
                                }
                            }
                        }
                    }
                    else if (current != null)
                    {
                        builder.Append(" ");
                        builder.Append(current);
                    }
                    substitute = builder.ToString();
                }
                else if (expr.StartsWith("*"))
                {
                    // Status display of active results
                    var filled = expr.ToLower().Trim().EndsWith("filled");
                    var builder = new StringBuilder();
                    if (match.Index > 0)
                    {
                        builder.AppendLine();
                    }
                    foreach (var entry in (from step in _fields where (!filled || !step.IsUnknown(state)) && step.Role == FieldRole.Value && step.Active(state) select step))
                    {
                        var format = new Prompter<T>(Template(entry, TemplateUsage.StatusFormat), _form, null);
                        builder.Append("* ").AppendLine(format.Prompt(state, entry).Prompt);
                    }
                    substitute = builder.ToString();
                }
                else if (expr.StartsWith("[") && expr.EndsWith("]"))
                {
                    // Generate a list from multiple fields
                    var paths = expr.Substring(1, expr.Length - 2).Split(' ');
                    var values = new List<Tuple<IField<T>, object, string>>();
                    foreach (var spec in paths)
                    {
                        if (!spec.StartsWith("{") || !spec.EndsWith("}"))
                        {
                            throw new ArgumentException("Only {<field>} references are allowed in lists.");
                        }
                        var formatArgs = spec.Substring(1, spec.Length - 2).Trim().Split(':');
                        var name = formatArgs[0];
                        if (name == string.Empty && field != null) name = field.Name;
                        var format = (formatArgs.Length > 1 ? "0:" + formatArgs[1] : "0");
                        var eltDesc = _fields.Field(name);
                        if (!eltDesc.IsUnknown(state))
                        {
                            var value = eltDesc.GetValue(state);
                            if (value.GetType() != typeof(string) && value.GetType().IsIEnumerable())
                            {
                                var eltValues = (value as System.Collections.IEnumerable);
                                foreach (var elt in eltValues)
                                {
                                    values.Add(Tuple.Create(eltDesc, elt, format));
                                }
                            }
                            else
                            {
                                values.Add(Tuple.Create(eltDesc, eltDesc.GetValue(state), format));
                            }
                        }
                    }
                    if (values.Count() > 0)
                    {
                        var elements = (from elt in values
                                        select Language.Normalize(ValueDescription(elt.Item1, elt.Item2, elt.Item3), _annotation.ValueCase)).ToArray();
                        substitute = Language.BuildList(elements, _annotation.Separator, _annotation.LastSeparator);
                    }
                }
                else if (expr.StartsWith("?"))
                {
                    // Conditional template
                    var subValue = ExpandTemplate(expr.Substring(1), currentChoice, null, state, field, args, ref buttons);
                    if (subValue == null)
                    {
                        substitute = string.Empty;
                    }
                    else
                    {
                        substitute = subValue;
                    }
                }
                else if (TryParseFormat(expr, out numeric))
                {
                    // Process ad hoc arg
                    if (numeric < args.Length && args[numeric] != null)
                    {
                        substitute = string.Format("{" + expr + "}", args);
                    }
                    else
                    {
                        foundUnspecified = true;
                        break;
                    }
                }
                else
                {
                    var formatArgs = expr.Split(':');
                    var name = formatArgs[0];
                    if (name == string.Empty && field != null) name = field.Name;
                    var pathDesc = _fields.Field(name);
                    if (pathDesc.IsUnknown(state))
                    {
                        if (noValue == null)
                        {
                            foundUnspecified = true;
                            break;
                        }
                        substitute = noValue;
                    }
                    else
                    {
                        var value = pathDesc.GetValue(state);
                        if (value.GetType() != typeof(string) && value.GetType().IsIEnumerable())
                        {
                            var values = (value as System.Collections.IEnumerable);
                            substitute = Language.BuildList(from elt in values.Cast<object>()
                                                            select Language.Normalize(ValueDescription(pathDesc, elt, "0"), _annotation.ValueCase),
                                _annotation.Separator, _annotation.LastSeparator);
                        }
                        else
                        {
                            var format = (formatArgs.Length > 1 ? "0:" + formatArgs[1] : "0");
                            substitute = ValueDescription(pathDesc, value, format);
                        }
                    }
                }
                response.Append(template.Substring(last, match.Index - last)).Append(substitute);
                last = match.Index + match.Length;
            }
            return (foundUnspecified ? null : response.Append(template.Substring(last, template.Length - last)).ToString());
        }

        private static bool TryParseFormat(string format, out int number)
        {
            var args = format.Split(':');
            return int.TryParse(args[0], out number);
        }

        private string ValueDescription(IField<T> field, object value, string format)
        {
            string result;
            if (format != "0")
            {
                result = string.Format("{" + format + "}", value);
            }
            else
            {
                result = field.Prompt.Recognizer.ValueDescription(value).Description;
            }
            return result;
        }

        private TemplateAttribute Template(IField<T> field, TemplateUsage usage)
        {
            return field == null
                ? _form.Configuration.Template(usage)
                : field.Template(usage);
        }

        private static readonly Regex _args = new Regex(@"{((?>[^{}]+|{(?<number>)|}(?<-number>))*(?(number)(?!)))}", RegexOptions.Compiled);
        private static readonly Regex _spaces = new Regex(@"(\S)( {2,})", RegexOptions.Compiled);
        private static readonly Regex _spacesPunc = new Regex(@"(?:\s+)(\.|\?)", RegexOptions.Compiled);
        private IForm<T> _form;
        private IFields<T> _fields;
        private TemplateBaseAttribute _annotation;
        private IRecognize<T> _recognizer;
    }
}
