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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Resource;
using Microsoft.Bot.Builder.Classic.ConnectorEx;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// The style of generated prompt
    /// </summary>
    public enum PromptStyle
    {
        /// <summary>
        /// Generate buttons for choices and let connector generate the right style based on channel capabilities
        /// </summary>
        Auto,

        /// <summary>
        /// Map choices to a list of suggested actions that depending on the channel will be a keyboard, quick replies or a 
        /// <see cref="HeroCard"/>.
        /// </summary>
        Keyboard,

        /// <summary>
        /// Show choices as Text.
        /// </summary>
        /// <remarks> The prompt decides if it should generate the text inline or perline based on number of choices.</remarks>
        AutoText,

        /// <summary>
        /// Show choices on the same line.
        /// </summary>
        Inline,

        /// <summary>
        /// Show choices with one per line.
        /// </summary>
        PerLine,

        /// <summary>
        /// Do not show possible choices in the prompt
        /// </summary>
        None
    }

    /// <summary>
    /// Options for <see cref="PromptDialog"/>.
    /// </summary>
    /// <typeparam name="T"> The type of the options.</typeparam>
    public interface IPromptOptions<T>
    {
        /// <summary>
        /// The prompt.
        /// </summary>
        string Prompt { get; }

        /// <summary>
        /// What to display on retry.
        /// </summary>
        string Retry { get; }

        /// <summary>
        /// Speak tag (SSML markup for text to speech)
        /// </summary>
        string Speak { get; }

        /// <summary>
        /// Retry Speak tag (SSML markup for text to speech)
        /// </summary>
        string RetrySpeak { get; }

        /// <summary>
        /// The choices to be returned when selected.
        /// </summary>
        IReadOnlyList<T> Options { get; }

        /// <summary>
        /// The choices and synonyms to be returned when selected.
        /// </summary>
        IReadOnlyDictionary<T, IReadOnlyList<T>> Choices { get; }

        /// <summary>
        /// The description of each possible option.
        /// </summary>
        /// <remarks>
        /// If this is null, then the descriptions will be the options converted to strings.
        /// Otherwise this should have the same number of values as Options and it contains the string to describe the value being selected.
        /// </remarks>
        IReadOnlyList<string> Descriptions { get; }

        /// <summary>
        /// What to display when user didn't say a valid response after <see cref="Attempts"/>.
        /// </summary>
        string TooManyAttempts { get; }

        /// <summary>
        /// Maximum number of attempts.
        /// </summary>
        int Attempts { set; get; }

        /// <summary>
        /// Styler of the prompt <see cref="Dialogs.PromptStyler"/>.
        /// </summary>
        PromptStyler PromptStyler { get; }

        /// <summary>
        /// Default retry prompt that is used if <see cref="Retry"/> is null.
        /// </summary>
        string DefaultRetry { get; set; }

        /// <summary>
        /// Default retry speak that is used if <see cref="RetrySpeak"/> is null.
        /// </summary>
        string DefaultRetrySpeak { get; set; }

        /// <summary>
        /// Entity Recognizer to parse the message content
        /// </summary>
        IPromptRecognizer Recognizer { get; }
    }

    /// <summary>
    /// Options for <see cref="PromptDialog"/>.
    /// </summary>
    /// <typeparam name="T"> The type of the options.</typeparam>
    [Serializable]
    public class PromptOptions<T> : IPromptOptions<T>
    {
        private PromptOptionsWithSynonyms<T> promptOptionsWithChoices;

        /// <summary>
        /// The prompt.
        /// </summary>
        public string Prompt { get { return this.promptOptionsWithChoices.Prompt; } }

        /// <summary>
        /// What to display on retry.
        /// </summary>
        public string Retry { get { return this.promptOptionsWithChoices.Retry; } }

        /// <summary>
        /// Speak tag (SSML markup for text to speech)
        /// </summary>
        public string Speak { get { return this.promptOptionsWithChoices.Speak; } }

        /// <summary>
        /// Retry Speak tag (SSML markup for text to speech)
        /// </summary>
        public string RetrySpeak { get { return this.promptOptionsWithChoices.RetrySpeak; } }

        /// <summary>
        /// The choices to be returned when selected.
        /// </summary>
        public IReadOnlyList<T> Options { get { return this.promptOptionsWithChoices.Options; } }

        /// <summary>
        /// The choices and synonyms to be returned when selected.
        /// </summary>
        public IReadOnlyDictionary<T, IReadOnlyList<T>> Choices { get { return this.promptOptionsWithChoices.Choices; } }

        /// <summary>
        /// The description of each possible option.
        /// </summary>
        /// <remarks>
        /// If this is null, then the descriptions will be the options converted to strings.
        /// Otherwise this should have the same number of values as Options and it contains the string to describe the value being selected.
        /// </remarks>
        public IReadOnlyList<string> Descriptions { get { return this.promptOptionsWithChoices.Descriptions; } }

        /// <summary>
        /// What to display when user didn't say a valid response after <see cref="Attempts"/>.
        /// </summary>
        public string TooManyAttempts { get { return this.promptOptionsWithChoices.TooManyAttempts; } }

        /// <summary>
        /// Maximum number of attempts.
        /// </summary>
        public int Attempts
        {
            set { this.promptOptionsWithChoices.Attempts = value; }
            get { return this.promptOptionsWithChoices.Attempts; }
        }

        /// <summary>
        /// Styler of the prompt <see cref="Dialogs.PromptStyler"/>.
        /// </summary>
        public PromptStyler PromptStyler { get { return this.promptOptionsWithChoices.PromptStyler; } }

        /// <summary>
        /// Default retry prompt that is used if <see cref="Retry"/> is null.
        /// </summary>
        public string DefaultRetry
        {
            set { this.promptOptionsWithChoices.DefaultRetry = value; }
            get { return this.promptOptionsWithChoices.DefaultRetry; }
        }

        /// <summary>
        /// Default retry speak that is used if <see cref="RetrySpeak"/> is null.
        /// </summary>
        public string DefaultRetrySpeak
        {
            set { this.promptOptionsWithChoices.DefaultRetrySpeak = value; }
            get { return this.promptOptionsWithChoices.DefaultRetrySpeak; }
        }

        /// <summary>
        /// Default <see cref="TooManyAttempts"/> string that is used if <see cref="TooManyAttempts"/> is null.
        /// </summary>
        protected string DefaultTooManyAttempts
        {
            get { return Resources.TooManyAttempts; }
        }

        /// <summary>
        /// Entity Recognizer to parse the message content
        /// </summary>
        public IPromptRecognizer Recognizer { get { return this.promptOptionsWithChoices.Recognizer; } }

        /// <summary>
        /// Constructs the prompt options.
        /// </summary>
        /// <param name="prompt"> The prompt.</param>
        /// <param name="retry"> What to display on retry.</param>
        /// <param name="tooManyAttempts"> What to display when user didn't say a valid response after <see cref="Attempts"/>.</param>
        /// <param name="options"> The prompt choice values.</param>
        /// <param name="attempts"> Maximum number of attempts.</param>
        /// <param name="promptStyler"> The prompt styler.</param>
        /// <param name="descriptions">Descriptions for each prompt.</param>
        /// <param name="speak"> The Speak tag (SSML markup for text to speech).</param>
        /// <param name="retrySpeak"> What to display on retry Speak (SSML markup for text to speech).</param>
        /// <param name="recognizer"> Entity Recognizer to parse the message content.</param>
        public PromptOptions(string prompt, string retry = null, string tooManyAttempts = null, IReadOnlyList<T> options = null, int attempts = 3, PromptStyler promptStyler = null, IReadOnlyList<string> descriptions = null, string speak = null, string retrySpeak = null, IPromptRecognizer recognizer = null)
        {
            var choices = options != null ? new ReadOnlyDictionary<T, IReadOnlyList<T>>(options.ToDictionary(x => x, x => (IReadOnlyList<T>)Enumerable.Empty<T>().ToList().AsReadOnly())) : null;
            this.promptOptionsWithChoices = new PromptOptionsWithSynonyms<T>(
                prompt,
                retry,
                tooManyAttempts,
                choices,
                attempts,
                promptStyler,
                descriptions,
                speak,
                retrySpeak,
                recognizer);
        }
    }

    /// <summary>
    /// Options with synonyms for <see cref="PromptDialog"/>.
    /// </summary>
    /// <typeparam name="T"> The type of the options.</typeparam>
    [Serializable]
    public class PromptOptionsWithSynonyms<T> : IPromptOptions<T>
    {
        /// <summary>
        /// The prompt.
        /// </summary>
        public string Prompt { get; }

        /// <summary>
        /// What to display on retry.
        /// </summary>
        public string Retry { get; }

        /// <summary>
        /// Speak tag (SSML markup for text to speech)
        /// </summary>
        public string Speak { get; }

        /// <summary>
        /// Retry Speak tag (SSML markup for text to speech)
        /// </summary>
        public string RetrySpeak { get; }

        /// <summary>
        /// The choices to be returned when selected.
        /// </summary>
        public IReadOnlyList<T> Options { get; }

        /// <summary>
        /// The choices and synonyms to be returned when selected.
        /// </summary>
        public IReadOnlyDictionary<T, IReadOnlyList<T>> Choices { get; }

        /// <summary>
        /// The description of each possible option.
        /// </summary>
        /// <remarks>
        /// If this is null, then the descriptions will be the options converted to strings.
        /// Otherwise this should have the same number of values as Options and it contains the string to describe the value being selected.
        /// </remarks>
        public IReadOnlyList<string> Descriptions { get; }

        /// <summary>
        /// What to display when user didn't say a valid response after <see cref="Attempts"/>.
        /// </summary>
        public string TooManyAttempts { get; }

        /// <summary>
        /// Maximum number of attempts.
        /// </summary>
        public int Attempts { set; get; }

        /// <summary>
        /// Styler of the prompt <see cref="Dialogs.PromptStyler"/>.
        /// </summary>
        public PromptStyler PromptStyler { get; }

        /// <summary>
        /// Default retry prompt that is used if <see cref="Retry"/> is null.
        /// </summary>
        public string DefaultRetry { get; set; }

        /// <summary>
        /// Default retry speak that is used if <see cref="RetrySpeak"/> is null.
        /// </summary>
        public string DefaultRetrySpeak { get; set; }

        /// <summary>
        /// Default <see cref="TooManyAttempts"/> string that is used if <see cref="TooManyAttempts"/> is null.
        /// </summary>
        protected string DefaultTooManyAttempts
        {
            get { return Resources.TooManyAttempts; }
        }

        /// <summary>
        /// Entity Recognizer to parse the message content
        /// </summary>
        public IPromptRecognizer Recognizer { get; }

        /// <summary>
        /// Constructs the prompt options.
        /// </summary>
        /// <param name="prompt"> The prompt.</param>
        /// <param name="retry"> What to display on retry.</param>
        /// <param name="tooManyAttempts"> What to display when user didn't say a valid response after <see cref="Attempts"/>.</param>
        /// <param name="choices"> The prompt choice values.</param>
        /// <param name="attempts"> Maximum number of attempts.</param>
        /// <param name="promptStyler"> The prompt styler.</param>
        /// <param name="descriptions">Descriptions for each prompt.</param>
        /// <param name="speak"> The Speak tag (SSML markup for text to speech).</param>
        /// <param name="retrySpeak"> What to display on retry Speak (SSML markup for text to speech).</param>
        /// <param name="recognizer"> Entity Recognizer to parse the message content.</param>
        public PromptOptionsWithSynonyms(string prompt, string retry = null, string tooManyAttempts = null, IReadOnlyDictionary<T, IReadOnlyList<T>> choices = null, int attempts = 3, PromptStyler promptStyler = null, IReadOnlyList<string> descriptions = null, string speak = null, string retrySpeak = null, IPromptRecognizer recognizer = null)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            this.Prompt = prompt;
            this.Retry = retry;
            this.Speak = speak;
            this.RetrySpeak = retrySpeak;
            this.TooManyAttempts = tooManyAttempts ?? this.DefaultTooManyAttempts;
            this.Attempts = attempts;
            this.Choices = choices;
            this.Options = this.Choices?.Keys.ToList().AsReadOnly();
            this.Descriptions = descriptions;
            this.DefaultRetry = prompt;
            this.DefaultRetrySpeak = speak;
            if (promptStyler == null)
            {
                promptStyler = new PromptStyler();
            }
            this.Recognizer = recognizer ?? new PromptRecognizer();
            this.PromptStyler = promptStyler;
        }
    }

    /// <summary>
    /// Styles a prompt
    /// </summary>
    [Serializable]
    public class PromptStyler
    {
        /// <summary>
        /// Style of the prompt <see cref="Dialogs.PromptStyle"/>.
        /// </summary>
        public readonly PromptStyle PromptStyle;

        public PromptStyler(PromptStyle promptStyle = PromptStyle.Auto)
        {
            this.PromptStyle = promptStyle;
        }

        /// <summary>
        /// <see cref="PromptStyler.Apply(ref IMessageActivity, string, string)"/>.
        /// </summary>
        /// <typeparam name="T"> The type of the options.</typeparam>
        /// <param name="message"> The message.</param>
        /// <param name="prompt"> The prompt.</param>
        /// <param name="options"> The options.</param>
        /// <param name="promptStyle"> The prompt style.</param>
        /// <param name="descriptions">Descriptions for each option.</param>
        /// <param name="speak"> The speak.</param>
        public static void Apply<T>(ref IMessageActivity message, string prompt, IReadOnlyList<T> options, PromptStyle promptStyle, IReadOnlyList<string> descriptions = null, string speak = null)
        {
            var styler = new PromptStyler(promptStyle);
            styler.Apply(ref message, prompt, options, descriptions, speak);
        }

        /// <summary>
        /// Style a prompt and populate the <see cref="IMessageActivity.Text"/>.
        /// </summary>
        /// <param name="message"> The message that will contain the prompt.</param>
        /// <param name="prompt"> The prompt.</param>
        /// <param name="speak"> The speak.</param>
        public virtual void Apply(ref IMessageActivity message, string prompt, string speak = null)
        {
            SetField.CheckNull(nameof(prompt), prompt);
            message.Text = prompt;
            message.Speak = speak;
            message.InputHint = InputHints.ExpectingInput;
        }

        /// <summary>
        /// Style a prompt and populate the message based on <see cref="PromptStyler.PromptStyle"/>.
        /// </summary>
        /// <typeparam name="T"> The type of the options.</typeparam>
        /// <param name="message"> The message that will contain the prompt.</param>
        /// <param name="prompt"> The prompt.</param>
        /// <param name="options"> The options.</param>
        /// <param name="descriptions">Descriptions to display for each option.</param>
        /// <param name="speak"> The speak.</param>
        /// <remarks>
        /// <typeparamref name="T"/> should implement <see cref="object.ToString"/> unless descriptions are supplied.
        /// </remarks>
        public virtual void Apply<T>(ref IMessageActivity message, string prompt, IReadOnlyList<T> options, IReadOnlyList<string> descriptions = null, string speak = null)
        {
            SetField.CheckNull(nameof(prompt), prompt);
            SetField.CheckNull(nameof(options), options);
            message.Speak = speak;
            message.InputHint = InputHints.ExpectingInput;
            if (descriptions == null)
            {
                descriptions = (from option in options select option.ToString()).ToList();
            }
            switch (PromptStyle)
            {
                case PromptStyle.Auto:
                case PromptStyle.Keyboard:
                    if (options != null && options.Any())
                    {
                        if (PromptStyle == PromptStyle.Keyboard)
                        {
                            message.SuggestedActions = new SuggestedActions(actions: options.GenerateButtons(descriptions));
                            message.Text = prompt;
                        }
                        else
                        {
                            message.AddHeroCard(prompt, options, descriptions);
                        }
                    }
                    else
                    {
                        message.Text = prompt;
                    }
                    break;
                case PromptStyle.AutoText:
                    Apply(ref message, prompt, options, options?.Count() > 4 ? PromptStyle.PerLine : PromptStyle.Inline, descriptions);
                    break;
                case PromptStyle.Inline:
                    //TODO: Refactor buildlist function to a more generic namespace when changing prompt to use recognizers.
                    message.Text = $"{prompt} {FormFlow.Advanced.Language.BuildList(descriptions, Resources.DefaultChoiceSeparator, Resources.DefaultChoiceLastSeparator)}";
                    break;
                case PromptStyle.PerLine:
                    message.Text = $"{prompt}{Environment.NewLine}{FormFlow.Advanced.Language.BuildList(descriptions.Select(description => $"* {description}"), Environment.NewLine, Environment.NewLine)}";
                    break;
                case PromptStyle.None:
                default:
                    message.Text = prompt;
                    break;
            }
        }
    }

    /// <summary>   Dialog factory for simple prompts. </summary>
    /// <remarks>The exception <see cref="TooManyAttemptsException"/> will be thrown if the number of allowed attempts is exceeded.</remarks>
    public class PromptDialog
    {
        /// <summary>   Prompt for a string. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        public static void Text(IDialogContext context, ResumeAfter<string> resume, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptString(prompt, retry, attempts);
            context.Call<string>(child, resume);
        }

        /// <summary>   Ask a yes/no question. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
        /// <param name="options">Button labels for yes/no choices.</param>
        /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptConfirm.Yes"/> or <see cref="PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
        public static void Confirm(IDialogContext context, ResumeAfter<bool> resume, string prompt, string retry = null, int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto, string[] options = null, string[][] patterns = null)
        {
            Confirm(context, resume, new PromptOptions<string>(prompt, retry, attempts: attempts, options: options ?? PromptConfirm.Options, promptStyler: new PromptStyler(promptStyle: promptStyle)), patterns);
        }

        /// <summary>
        /// Ask a yes/no questions.
        /// </summary>
        /// <param name="context"> The dialog context.</param>
        /// <param name="resume"> Resume handler.</param>
        /// <param name="promptOptions"> The options for the prompt, <see cref="IPromptOptions{T}"/>.</param>
        /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptConfirm.Yes"/> or <see cref="PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
        public static void Confirm(IDialogContext context, ResumeAfter<bool> resume, IPromptOptions<string> promptOptions, string[][] patterns = null)
        {
            var child = new PromptConfirm(promptOptions, patterns);
            context.Call<bool>(child, resume);
        }

        /// <summary>   Prompt for a long. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        /// <param name="speak">    Speak tag (SSML markup for text to speech)</param>
        /// <param name="max">      Maximum value.</param>
        /// <param name="min">      Minimun value.</param>
        public static void Number(IDialogContext context, ResumeAfter<long> resume, string prompt, string retry = null, int attempts = 3, string speak = null, long? min = null, long? max = null)
        {
            var child = new PromptInt64(prompt, retry, attempts, speak, min, max);
            context.Call<long>(child, resume);
        }

        /// <summary>   Prompt for a double. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        /// <param name="speak">    Speak tag (SSML markup for text to speech)</param>
        /// <param name="max">      Maximum value.</param>
        /// <param name="min">      Minimun value.</param>
        public static void Number(IDialogContext context, ResumeAfter<double> resume, string prompt, string retry = null, int attempts = 3, string speak = null, double? min = null, double? max = null)
        {
            var child = new PromptDouble(prompt, retry, attempts, speak, min, max);
            context.Call<double>(child, resume);
        }

        /// <summary>   Prompt for one of a set of choices. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="options">  The possible options all of which must be convertible to a string.</param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
        /// <param name="descriptions">Descriptions to display for choices.</param>
        public static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, IEnumerable<T> options, string prompt, string retry = null, int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto, IEnumerable<string> descriptions = null)
        {
            Choice(context, resume, new PromptOptions<T>(prompt, retry, attempts: attempts, options: options.ToList(), promptStyler: new PromptStyler(promptStyle), descriptions: descriptions?.ToList()));
        }

        /// <summary>   Prompt for one of a set of choices. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="choices"> Dictionary with the options to choose from as a key and their synonyms as a value.</param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
        /// <param name="descriptions">Descriptions to display for choices.</param>
        /// <param name="recognizeChoices">(Optional) if true, the prompt will attempt to recognize the selected value using the choices themselves. The default value is "true".</param>
        /// <param name="recognizeNumbers">(Optional) if true, the prompt will attempt to recognize numbers in the users utterance as the index of the choice to return. The default value is "true".</param>
        /// <param name="recognizeOrdinals">(Optional) if true, the prompt will attempt to recognize ordinals like "the first one" or "the second one" as the index of the choice to return. The default value is "true".</param>
        /// <param name="minScore">(Optional) minimum score from 0.0 - 1.0 needed for a recognized choice to be considered a match. The default value is "0.4".</param>
        public static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, IDictionary<T, IEnumerable<T>> choices, string prompt, string retry = null, int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto, IEnumerable<string> descriptions = null, bool recognizeChoices = true, bool recognizeNumbers = true, bool recognizeOrdinals = true, double minScore = 0.4)
        {
            Choice(context, resume, new PromptOptionsWithSynonyms<T>(prompt, retry, attempts: attempts, choices: choices.ToDictionary(x => x.Key, x => (IReadOnlyList<T>)x.Value.ToList().AsReadOnly()), promptStyler: new PromptStyler(promptStyle), descriptions: descriptions?.ToList()), recognizeChoices, recognizeNumbers, recognizeOrdinals, minScore: minScore);
        }

        /// <summary>
        /// Prompt for one of a set of choices.
        /// </summary>
        /// <remarks><typeparamref name="T"/> should implement <see cref="object.ToString"/></remarks>
        /// <typeparam name="T"> The type of the options.</typeparam>
        /// <param name="context"> The dialog context.</param>
        /// <param name="resume"> Resume handler.</param>
        /// <param name="promptOptions"> The prompt options.</param>
        /// <param name="recognizeChoices">(Optional) if true, the prompt will attempt to recognize the selected value using the choices themselves. The default value is "true".</param>
        /// <param name="recognizeNumbers">(Optional) if true, the prompt will attempt to recognize numbers in the users utterance as the index of the choice to return. The default value is "true".</param>
        /// <param name="recognizeOrdinals">(Optional) if true, the prompt will attempt to recognize ordinals like "the first one" or "the second one" as the index of the choice to return. The default value is "true".</param>
        /// <param name="minScore">(Optional) minimum score from 0.0 - 1.0 needed for a recognized choice to be considered a match. The default value is "0.4".</param>
        public static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, IPromptOptions<T> promptOptions, bool recognizeChoices = true, bool recognizeNumbers = true, bool recognizeOrdinals = true, double minScore = 0.4)
        {
            var child = new PromptChoice<T>(promptOptions, recognizeChoices, recognizeNumbers, recognizeOrdinals, minScore);
            context.Call<T>(child, resume);
        }

        /// <summary>
        /// Prompt for an attachment
        /// </summary>
        /// <param name="context"> The dialog context. </param>
        /// <param name="resume"> Resume handler. </param>
        /// <param name="prompt"> The prompt to show to the user. </param>
        /// <param name="contentTypes">The optional content types the attachment type should be part of</param>
        /// <param name="retry"> What to show on retry</param>
        /// <param name="attempts"> The number of times to retry</param>
        public static void Attachment(IDialogContext context, ResumeAfter<IEnumerable<Attachment>> resume, string prompt, IEnumerable<string> contentTypes = null, string retry = null, int attempts = 3)
        {
            var child = new PromptAttachment(prompt, retry, attempts, contentTypes);
            context.Call<IEnumerable<Attachment>>(child, resume);
        }

        /// <summary>   Prompt for a text string. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Text(IDialogContext, ResumeAfter{string}, string, string, int)"/>.</remarks>
        [Serializable]
        public class PromptString : Prompt<string, string>
        {
            /// <summary>   Constructor for a prompt string dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            public PromptString(string prompt, string retry, int attempts)
                : this(new PromptOptions<string>(prompt, retry, attempts: attempts)) { }

            /// <summary>   Constructor for a prompt string dialog. </summary>
            /// <param name="promptOptions"> THe prompt options.</param>
            public PromptString(IPromptOptions<string> promptOptions)
                : base(promptOptions)
            {
                this.promptOptions.DefaultRetry = this.DefaultRetry;
            }

            protected internal override bool TryParse(IMessageActivity message, out string result)
            {
                if (!string.IsNullOrWhiteSpace(message.Text))
                {
                    result = message.Text;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            public string DefaultRetry
            {
                get
                {
                    return Resources.PromptRetry + Environment.NewLine + this.promptOptions.Prompt;
                }
            }
        }

        /// <summary>   Prompt for a confirmation. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Confirm(IDialogContext, ResumeAfter{bool}, string, string, int, PromptStyle, string[], string[][])"/>.</remarks>
        [Serializable]
        public class PromptConfirm : Prompt<bool, string>
        {
            private PromptChoice<string> innerPromptChoice;

            private string[][] patterns;

            /// <summary>
            /// Index of yes descriptions.
            /// </summary>
            public const int Yes = 0;

            /// <summary>
            /// Index of no descriptions.
            /// </summary>
            public const int No = 1;

            /// <summary>
            /// The yes, no choice labels for confirmation prompt
            /// </summary>
            public static string[] Options
            {
                get
                {
                    return new string[] { Resources.MatchYes.SplitList().First(), Resources.MatchNo.SplitList().First() };
                }
            }

            /// <summary>
            /// The patterns for matching yes/no responses in the confirmation prompt.
            /// </summary>
            public static string[][] Patterns
            {
                get
                {
                    return new string[][] { Resources.MatchYes.SplitList(), Resources.MatchNo.SplitList() };
                }
            }

            /// <summary>   Constructor for a prompt confirmation dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
            /// <param name="options">Names for yes and no  options.</param>
            /// <param name="patterns">Yes and no alternatives for matching input where first dimension is either <see cref="PromptConfirm.Yes"/> or <see cref="PromptConfirm.No"/> and the arrays are alternative strings to match.</param>
            public PromptConfirm(string prompt, string retry, int attempts, PromptStyle promptStyle = PromptStyle.Auto, string[] options = null, string[][] patterns = null)
                : this(new PromptOptions<string>(prompt, retry, attempts: attempts, options: options ?? Options, promptStyler: new PromptStyler(promptStyle)), patterns)
            {
            }

            /// <summary>
            /// Constructor for a prompt confirmation dialog.
            /// </summary>
            /// <param name="promptOptions"> THe prompt options.</param>
            /// <param name="patterns"></param>
            public PromptConfirm(IPromptOptions<string> promptOptions, string[][] patterns = null)
                : base(promptOptions)
            {
                this.patterns = patterns ?? Patterns;
                this.promptOptions.DefaultRetry = this.DefaultRetry;

                var choices = new Dictionary<string, IReadOnlyList<string>>
                {
                    { Yes.ToString(), this.patterns[Yes].Select(x => x.ToLowerInvariant()).ToList() },
                    { No.ToString(), this.patterns[No].Select(x => x.ToLowerInvariant()).ToList() }
                };

                var promptChoiceOptions = new PromptOptionsWithSynonyms<string>(
                    promptOptions.Prompt,
                    promptOptions.Retry,
                    promptOptions.TooManyAttempts,
                    choices,
                    promptOptions.Attempts,
                    promptOptions.PromptStyler,
                    promptOptions.Descriptions,
                    promptOptions.Speak,
                    promptOptions.RetrySpeak,
                    promptOptions.Recognizer);

                this.innerPromptChoice = new PromptChoice<string>(promptChoiceOptions, recognizeNumbers: false, recognizeOrdinals: false);
            }

            protected internal override bool TryParse(IMessageActivity message, out bool result)
            {
                string entity = string.Empty;

                var innerResult = this.innerPromptChoice.TryParse(message, out entity);
                result = entity == Yes.ToString();

                return innerResult;
            }

            public string DefaultRetry
            {
                get
                {
                    return Resources.PromptRetry + Environment.NewLine + this.promptOptions.Prompt;
                }
            }
        }

        /// <summary>   Prompt for a Int64 </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Number(IDialogContext, ResumeAfter{long}, string, string, int, string, long?, long?)"/>.</remarks>
        [Serializable]
        public class PromptInt64 : Prompt<long, long>
        {
            /// <summary>
            /// (Optional) Minimum value allowed.
            /// </summary>
            public long? Min { get; }

            /// <summary>
            /// (Optional) Maximum value allowed.
            /// </summary>
            public long? Max { get; }

            /// <summary>   Constructor for a prompt int64 dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="speak">    Speak tag (SSML markup for text to speech)</param>
            /// <param name="max">      Maximum value.</param>
            /// <param name="min">      Minimun value.</param>
            public PromptInt64(string prompt, string retry, int attempts, string speak = null, long? min = null, long? max = null)
                : this(new PromptOptions<long>(prompt, retry, attempts: attempts, speak: speak), min, max) { }

            /// <summary>   Constructor for a prompt int64 dialog. </summary>
            /// <param name="promptOptions"> THe prompt options.</param>
            /// <param name="max">Maximum value.</param>
            /// <param name="min">Minimun value.</param>
            public PromptInt64(PromptOptions<long> promptOptions, long? min = null, long? max = null)
                : base(promptOptions)
            {
                this.Min = min;
                this.Max = max;
            }

            protected internal override bool TryParse(IMessageActivity message, out Int64 result)
            {
                var matches = this.promptOptions.Recognizer.RecognizeIntegerInRange(message, this.Min, this.Max);
                var topMatch = matches?.MaxBy(x => x.Score);
                if (topMatch != null && topMatch.Score > 0)
                {
                    result = topMatch.Entity;
                    return true;
                }
                result = 0;
                return false;
            }
        }

        /// <summary>   Prompt for a double. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Number(IDialogContext, ResumeAfter{double}, string, string, int, string, double?, double?)"/>.</remarks>
        [Serializable]
        public class PromptDouble : Prompt<double, double>
        {
            /// <summary>
            /// (Optional) Minimum value allowed.
            /// </summary>
            public double? Min { get; }

            /// <summary>
            /// (Optional) Maximum value allowed.
            /// </summary>
            public double? Max { get; }

            /// <summary>   Constructor for a prompt double dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="speak">    Speak tag (SSML markup for text to speech)</param>
            /// <param name="max">      Maximum value.</param>
            /// <param name="min">      Minimun value.</param>
            public PromptDouble(string prompt, string retry, int attempts, string speak = null, double? min = null, double? max = null)
                : this(new PromptOptions<double>(prompt, retry, attempts: attempts, speak: speak), min, max) { }

            /// <summary>   Constructor for a prompt double dialog. </summary>
            /// <param name="promptOptions"> THe prompt options.</param>
            /// <param name="max">Maximum value.</param>
            /// <param name="min">Minimun value.</param>
            public PromptDouble(PromptOptions<double> promptOptions, double? min = null, double? max = null)
                : base(promptOptions)
            {
                this.Min = min;
                this.Max = max;
            }

            protected internal override bool TryParse(IMessageActivity message, out double result)
            {
                var matches = this.promptOptions.Recognizer.RecognizeDoubleInRange(message, this.Min, this.Max);
                var topMatch = matches?.MaxBy(x => x.Score);
                if (topMatch != null && topMatch.Score > 0)
                {
                    result = topMatch.Entity;
                    return true;
                }
                result = 0;
                return false;
            }
        }

        /// <summary>   Prompt for a choice from a set of choices. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Choice{T}(IDialogContext, ResumeAfter{T}, IEnumerable{T}, string, string, int, PromptStyle, IEnumerable{string})"/>.</remarks>
        [Serializable]
        public class PromptChoice<T> : Prompt<T, T>
        {
            private bool recognizeChoices;
            private bool recognizeNumbers;
            private bool recognizeOrdinals;
            private double minScore;

            /// <summary>   Constructor for a prompt choice dialog. </summary>
            /// <param name="options">Enumerable of the options to choose from.</param>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
            /// <param name="descriptions">Descriptions to show for each option.</param>
            /// <param name="recognizeChoices">(Optional) if true, the prompt will attempt to recognize the selected value using the choices themselves. The default value is "true".</param>
            /// <param name="recognizeNumbers">(Optional) if true, the prompt will attempt to recognize numbers in the users utterance as the index of the choice to return. The default value is "true".</param>
            /// <param name="recognizeOrdinals">(Optional) if true, the prompt will attempt to recognize ordinals like "the first one" or "the second one" as the index of the choice to return. The default value is "true".</param>
            /// <param name="minScore">(Optional) minimum score from 0.0 - 1.0 needed for a recognized choice to be considered a match. The default value is "0.4".</param>
            public PromptChoice(IEnumerable<T> options, string prompt, string retry, int attempts, PromptStyle promptStyle = PromptStyle.Auto, IEnumerable<string> descriptions = null, bool recognizeChoices = true, bool recognizeNumbers = true, bool recognizeOrdinals = true, double minScore = 0.4)
                : this(new PromptOptions<T>(prompt, retry, options: options.ToList(), attempts: attempts, promptStyler: new PromptStyler(promptStyle), descriptions: descriptions?.ToList()), recognizeChoices, recognizeNumbers, recognizeOrdinals, minScore)
            {
            }

            /// <summary>   Constructor for a prompt choice dialog. </summary>
            /// <param name="choices">Dictionary with the options to choose from as a key and their synonyms as a value.</param>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
            /// <param name="descriptions">Descriptions to show for each option.</param>
            /// <param name="recognizeChoices">(Optional) if true, the prompt will attempt to recognize the selected value using the choices themselves. The default value is "true".</param>
            /// <param name="recognizeNumbers">(Optional) if true, the prompt will attempt to recognize numbers in the users utterance as the index of the choice to return. The default value is "true".</param>
            /// <param name="recognizeOrdinals">(Optional) if true, the prompt will attempt to recognize ordinals like "the first one" or "the second one" as the index of the choice to return. The default value is "true".</param>
            /// <param name="minScore">(Optional) minimum score from 0.0 - 1.0 needed for a recognized choice to be considered a match. The default value is "0.4".</param>
            public PromptChoice(IDictionary<T, IEnumerable<T>> choices, string prompt, string retry, int attempts, PromptStyle promptStyle = PromptStyle.Auto, IEnumerable<string> descriptions = null, bool recognizeChoices = true, bool recognizeNumbers = true, bool recognizeOrdinals = true, double minScore = 0.4)
                : this(new PromptOptionsWithSynonyms<T>(prompt, retry, choices: choices.ToDictionary(x => x.Key, x => (IReadOnlyList<T>)x.Value.ToList().AsReadOnly()), attempts: attempts, promptStyler: new PromptStyler(promptStyle), descriptions: descriptions?.ToList()), recognizeChoices, recognizeNumbers, recognizeOrdinals, minScore)
            {
            }

            /// <summary>
            /// Constructs a choice dialog.
            /// </summary>
            /// <param name="promptOptions"> The prompt options</param>s
            /// <param name="recognizeChoices">(Optional) if true, the prompt will attempt to recognize the selected value using the choices themselves. The default value is "true".</param>
            /// <param name="recognizeNumbers">(Optional) if true, the prompt will attempt to recognize numbers in the users utterance as the index of the choice to return. The default value is "true".</param>
            /// <param name="recognizeOrdinals">(Optional) if true, the prompt will attempt to recognize ordinals like "the first one" or "the second one" as the index of the choice to return. The default value is "true".</param>
            /// <param name="minScore">(Optional) minimum score from 0.0 - 1.0 needed for a recognized choice to be considered a match. The default value is "0.4".</param>
            public PromptChoice(IPromptOptions<T> promptOptions, bool recognizeChoices = true, bool recognizeNumbers = true, bool recognizeOrdinals = true, double minScore = 0.4)
                : base(promptOptions)
            {
                SetField.CheckNull(nameof(promptOptions.Choices), promptOptions.Choices);
                this.recognizeChoices = recognizeChoices;
                this.recognizeNumbers = recognizeNumbers;
                this.recognizeOrdinals = recognizeOrdinals;
                this.minScore = minScore;
            }

            protected internal override bool TryParse(IMessageActivity message, out T result)
            {
                if (!string.IsNullOrWhiteSpace(message.Text))
                {
                    var topScore = 0.0;
                    T topEntity = default(T);
                    if (recognizeChoices)
                    {
                        var options = new PromptRecognizeChoicesOptions { AllowPartialMatches = true };
                        var entityMatches = this.promptOptions.Recognizer.RecognizeChoices<T>(message, this.promptOptions.Choices, options);
                        var entityWinner = entityMatches.MaxBy(x => x.Score) ?? new RecognizeEntity<T>();
                        topScore = entityWinner.Score;
                        topEntity = entityWinner.Entity;
                    }

                    if (recognizeNumbers)
                    {
                        var cardinalMatches = this.promptOptions.Recognizer.RecognizeIntegerInRange(message, 1, this.promptOptions.Choices.Count);
                        var cardinalWinner = cardinalMatches.MaxBy(x => x.Score) ?? new RecognizeEntity<long>();
                        if (topScore < cardinalWinner.Score)
                        {
                            var index = (int)cardinalWinner.Entity - 1;
                            topScore = cardinalWinner.Score;
                            topEntity = this.promptOptions.Choices.Keys.ElementAt(index);
                        }
                    }

                    if (recognizeOrdinals)
                    {
                        var ordinalMatches = this.promptOptions.Recognizer.RecognizeOrdinals(message);
                        var ordinalWinner = ordinalMatches.MaxBy(x => x.Score) ?? new RecognizeEntity<long>();
                        if (topScore < ordinalWinner.Score)
                        {
                            var index = ordinalWinner.Entity > 0 ? (int)ordinalWinner.Entity - 1 : this.promptOptions.Choices.Count + (int)ordinalWinner.Entity;
                            if (index >= 0 && index < this.promptOptions.Choices.Count)
                            {
                                topScore = ordinalWinner.Score;
                                topEntity = this.promptOptions.Choices.Keys.ElementAt(index);
                            }
                        }
                    }

                    if (topScore >= this.minScore && topScore > 0)
                    {
                        result = topEntity;
                        return true;
                    }
                }

                result = default(T);
                return false;
            }
        }

        /// <summary> Prompt for an attachment</summary>
        /// <remarks> Normally used through <see cref="PromptDialog.Attachment(IDialogContext, ResumeAfter{IEnumerable{Attachment}}, string, IEnumerable{string}, string, int)"/>.</remarks>
        [Serializable]
        public class PromptAttachment : Prompt<IEnumerable<Attachment>, Attachment>
        {
            public IEnumerable<string> ContentTypes
            {
                get;
                private set;
            }

            /// <summary>   Constructor for a prompt attachment dialog. </summary> 
            /// <param name="prompt">   The prompt. </param> 
            /// <param name="retry">    What to display on retry. </param> 
            /// <param name="attempts"> The optional content types the attachment type should be part of.</param>
            /// <param name="contentTypes"> The content types that is used to filter the attachments. Null implies any content type.</param>
            public PromptAttachment(string prompt, string retry, int attempts, IEnumerable<string> contentTypes = null)
                : base(new PromptOptionsWithSynonyms<Attachment>(prompt, retry, attempts: attempts))
            {
                this.ContentTypes = contentTypes ?? new List<string>();
            }

            protected internal override bool TryParse(IMessageActivity message, out IEnumerable<Attachment> result)
            {
                if (message.Attachments != null && message.Attachments.Any())
                {
                    // Retrieve attachments corresponding to content types if any
                    result = ContentTypes.Any() ? message.Attachments.Join(ContentTypes, a => a.ContentType, c => c, (a, c) => a)
                                                         : message.Attachments;
                    return result != null && result.Any();
                }
                else
                {
                    result = null;
                    return false;
                }
            }
        }

    }

    public static partial class Extensions
    {
        /// <summary>
        /// Generates buttons from options and add them to the message.
        /// </summary>
        /// <remarks>
        /// <typeparamref name="T"/> should implement ToString().
        /// </remarks>
        /// <typeparam name="T"> Type of the options.</typeparam>
        /// <param name="message"> The message that the buttons will be added to.</param>
        /// <param name="text"> The text in the <see cref="HeroCard"/>.</param>
        /// <param name="options"> The options that cause generation of buttons.</param>
        /// <param name="descriptions">Descriptions for each option.</param>
        public static void AddHeroCard<T>(this IMessageActivity message, string text, IEnumerable<T> options, IEnumerable<string> descriptions = null)
        {
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Attachments = options.GenerateHeroCard(text, descriptions);
        }

        /// <summary>
        /// Generates buttons from options and add them to the message.
        /// </summary>
        /// <remarks>
        /// <typeparamref name="T"/> should implement ToString().
        /// </remarks>
        /// <typeparam name="T"> Type of the options.</typeparam>
        /// <param name="message"> The message that the buttons will be added to.</param>
        /// <param name="text"> The text in the <see cref="HeroCard"/>.</param>
        /// <param name="options"> The options that cause generation of buttons.</param>
        /// <param name="descriptions">Descriptions for each option.</param>
        public static void AddKeyboardCard<T>(this IMessageActivity message, string text, IEnumerable<T> options,
            IEnumerable<string> descriptions = null)
        {
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Attachments = options.GenerateKeyboardCard(text, descriptions);
        }

        internal static IList<Attachment> GenerateHeroCard<T>(this IEnumerable<T> options, string text, IEnumerable<string> descriptions = null)
        {
            var attachments = new List<Attachment>
            {
                new HeroCard(text: text, buttons: options.GenerateButtons(descriptions)).ToAttachment()
            };

            return attachments;
        }

#pragma warning disable CS0618
        internal static IList<Attachment> GenerateKeyboardCard<T>(this IEnumerable<T> options, string text, IEnumerable<string> descriptions = null)
        {
            var attachments = new List<Attachment>
            {
                new KeyboardCard(text: text, buttons: options.GenerateButtons(descriptions)).ToAttachment()
            };

            return attachments;
        }
#pragma warning restore CS0618

        internal static IList<CardAction> GenerateButtons<T>(this IEnumerable<T> options,
            IEnumerable<string> descriptions = null)
        {
            var actions = new List<CardAction>();
            int i = 0;
            var adescriptions = descriptions?.ToArray();
            foreach (var option in options)
            {
                var title = (adescriptions == null ? option.ToString() : adescriptions[i]);
                actions.Add(new CardAction
                {
                    Title = title,
                    Type = ActionTypes.ImBack,
                    Value = option.ToString()
                });
                ++i;
            }
            return actions;
        }
    }
}

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{

    [Serializable]
    public abstract class Prompt<T, U> : IDialog<T>
    {
        protected readonly IPromptOptions<U> promptOptions;

        public Prompt(IPromptOptions<U> promptOptions)
        {
            SetField.NotNull(out this.promptOptions, nameof(promptOptions), promptOptions);
        }

        async Task IDialog<T>.StartAsync(IDialogContext context)
        {
            await context.PostAsync(this.MakePrompt(context, promptOptions.Prompt, promptOptions.Choices?.Keys.ToList().AsReadOnly(), promptOptions.Descriptions, promptOptions.Speak));
            context.Wait(MessageReceivedAsync);
        }

        protected virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            T result;
            if (this.TryParse(await message, out result))
            {
                context.Done(result);
            }
            else
            {
                --promptOptions.Attempts;
                if (promptOptions.Attempts >= 0)
                {
                    await context.PostAsync(this.MakePrompt(context, promptOptions.Retry ?? promptOptions.DefaultRetry, promptOptions.Choices?.Keys.ToList().AsReadOnly(), promptOptions.Descriptions, promptOptions.RetrySpeak ?? promptOptions.DefaultRetrySpeak));
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    //too many attempts, throw.
                    await context.PostAsync(this.MakePrompt(context, promptOptions.TooManyAttempts));
                    throw new TooManyAttemptsException(promptOptions.TooManyAttempts);
                }
            }
        }

        protected internal abstract bool TryParse(IMessageActivity message, out T result);

        protected virtual IMessageActivity MakePrompt(IDialogContext context, string prompt, IReadOnlyList<U> options = null, IReadOnlyList<string> descriptions = null, string speak = null)
        {
            var msg = context.MakeMessage();
            if (options != null && options.Count > 0)
            {
                promptOptions.PromptStyler.Apply(ref msg, prompt, options, descriptions, speak);
            }
            else
            {
                promptOptions.PromptStyler.Apply(ref msg, prompt, speak);
            }
            return msg;
        }
    }
}