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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Chronic;
using System.Threading;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    /// <summary>
    /// Recognizer for enumerated values.
    /// </summary>
    public sealed class RecognizeEnumeration<T> : IRecognize<T>
        where T : class
    {
        /// <summary>
        /// Delegate for mapping from a C# value to it's description.
        /// </summary>
        /// <param name="value">C# value to get description for.</param>
        /// <returns>Description of C# value.</returns>
        public delegate DescribeAttribute DescriptionDelegate(object value);

        /// <summary>
        /// Delegate to return the terms to match on for a C# value.
        /// </summary>
        /// <param name="value">C# value to get terms for.</param>
        /// <returns>Enumeration of regular expressions to match on for value.</returns>
        public delegate IEnumerable<string> TermsDelegate(object value);

        /// <summary>
        /// Constructor based on <see cref="IField{T}"/>.
        /// </summary>
        /// <param name="field">Field with enumerated values.</param>
        public RecognizeEnumeration(IField<T> field)
        {
            _form = field.Form;
            _allowNumbers = field.AllowNumbers;
            _description = field.FieldDescription.Description;
            _terms = field.FieldTerms.ToArray();
            _values = field.Values.ToArray();
            _valueDescriptions = field.ValueDescriptions.ToArray();
            _descriptionDelegate = (value) => field.ValueDescription(value);
            _termsDelegate = (value) => field.Terms(value);
            _helpFormat = field.Template(field.AllowNumbers
                ? (field.AllowsMultiple ? TemplateUsage.EnumManyNumberHelp : TemplateUsage.EnumOneNumberHelp)
                : (field.AllowsMultiple ? TemplateUsage.EnumManyWordHelp : TemplateUsage.EnumOneWordHelp));
            _noPreference = field.Optional ? field.Form.Configuration.NoPreference : null;
            _currentChoice = field.Form.Configuration.CurrentChoice.FirstOrDefault();
            BuildPerValueMatcher(from term in field.Form.Configuration.CurrentChoice select Regex.Escape(term.Trim()).Replace(" ", @"\s+"));
        }

        public object[] PromptArgs()
        {
            return new object[0];
        }

        public IEnumerable<object> Values()
        {
            return _values;
        }

        public IEnumerable<DescribeAttribute> ValueDescriptions()
        {
            return _valueDescriptions;
        }

        public DescribeAttribute ValueDescription(object value)
        {
            return _descriptionDelegate(value);
        }

        public IEnumerable<string> ValidInputs(object value)
        {
            return _termsDelegate(value);
        }

        public string Help(T state, object defaultValue)
        {
            var values = (from val in _valueDescriptions select val.Description);
            var max = _max;
            if (_noPreference != null)
            {
                values = values.Union(new string[] { _noPreference.First() });
                if (defaultValue == null)
                {
                    --max;
                }
            }
            if ((defaultValue != null || _noPreference != null) && _currentChoice != null)
            {
                values = values.Union(new string[] { _currentChoice });
            }
            var args = new List<object>();
            if (_allowNumbers)
            {
                args.Add(1);
                args.Add(max);
            }
            else
            {
                args.Add(null);
                args.Add(null);
            }
            args.Add(Language.BuildList(from val in values select Language.Normalize(val, _helpFormat.ChoiceCase), _helpFormat.ChoiceSeparator, _helpFormat.ChoiceLastSeparator));
            return new Prompter<T>(_helpFormat, _form, this).Prompt(state, null, args.ToArray()).Prompt;
        }

        public IEnumerable<TermMatch> Matches(IMessageActivity input, object defaultValue)
        {
            var inputText = MessageActivityHelper.GetSanitizedTextInput(input);

            // if the user hit enter on an optional prompt, then consider taking the current choice as a low confidence option
            bool userSkippedPrompt = string.IsNullOrWhiteSpace(inputText) && (defaultValue != null || _noPreference != null);
            if (userSkippedPrompt)
            {
                yield return new TermMatch(0, inputText.Length, 1.0, defaultValue);
            }

            foreach (var expression in _expressions)
            {
                double maxWords = expression.MaxWords;
                foreach (Match match in expression.Expression.Matches(inputText))
                {
                    var group1 = match.Groups[1];
                    var group2 = match.Groups[2];
                    object newValue;
                    if (group1.Success)
                    {
                        if (ConvertSpecial(expression.Value, defaultValue, out newValue))
                        {
                            yield return new TermMatch(group1.Index, group1.Length, 1.0, newValue);
                        }
                    }
                    if (group2.Success)
                    {
                        var words = group2.Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                        var confidence = System.Math.Min(words / maxWords, 1.0);
                        if (ConvertSpecial(expression.Value, defaultValue, out newValue))
                        {
                            yield return new TermMatch(group2.Index, group2.Length, confidence, newValue);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("RecognizeEnumeration({0}", _description);
            builder.Append(" [");
            foreach (var description in _valueDescriptions)
            {
                builder.Append(" ");
                builder.Append(description);
            }
            builder.Append("])");
            return builder.ToString();
        }

        private enum Special { CurrentChoice, NoPreference };

        // Word character, any word character, any digit, any positive group over word characters
        private const string WORD = @"(\w|\\w|\\d|(\[(?>(\w|-)+|\[(?<number>)|\](?<-number>))*(?(number)(?!))\]))";
        // Starts with matching word character
        private static Regex _wordStart = new Regex($@"^{WORD}", RegexOptions.Compiled);
        // Ends with matching word character with optional repetitions
        private static Regex _wordEnd = new Regex($@"{WORD}(\?|\*|\+|\{{\d+\}}|\{{,\d+\}}|\{{\d+,\d+\}})?$", RegexOptions.Compiled);

        private void BuildPerValueMatcher(IEnumerable<string> currentChoice)
        {
            if (currentChoice != null)
            {
                // 0 is reserved for current default if any
                AddExpression(0, Special.CurrentChoice, currentChoice, _allowNumbers);
            }
            var n = 1;
            foreach (var value in _values)
            {
                n = AddExpression(n, value, _termsDelegate(value), _allowNumbers);
            }
            if (_noPreference != null)
            {
                // Add recognizer for no preference
                n = AddExpression(n, Special.NoPreference, _noPreference, _allowNumbers);
            }
            if (_terms != null && _terms.Count() > 0)
            {
                // Add field terms to help disambiguate
                AddExpression(n, SpecialValues.Field, _terms, false);
            }
            _max = n - 1;
        }

        private int NumberOfWords(string regex)
        {
            return regex.Split(new string[] { @"\s", " " }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        // Generate a regex with 3 parts:
        // Group 1: startWord <number> endWord
        // Group 2: (all word terms) with optional start/end words
        // If a group is empty, match on NOMATCH constant.
        private const string NOMATCH = "__qqqq__";
        private int AddExpression(int n, object value, IEnumerable<string> terms, bool allowNumbers)
        {
            var orderedTerms = (from term in terms orderby term.Length descending select term).ToArray();
            var words = new StringBuilder();
            var first = true;
            int maxWords = 0;
            if (orderedTerms.Length > 0)
            {
                maxWords = terms.Max(NumberOfWords);
                foreach (var term in orderedTerms)
                {
                    var nterm = term.Trim().Replace(" ", @"\s+");
                    if (nterm != string.Empty)
                    {
                        if (first)
                        {
                            first = false;
                            words.Append('(');
                        }
                        else
                        {
                            words.Append('|');
                        }
                        words.Append("(?:");
                        if (_wordStart.Match(nterm).Success && _wordEnd.Match(nterm).Success)
                        {
                            words.Append($@"\b{nterm}\b");
                        }
                        else
                        {
                            words.Append(nterm);
                        }
                        words.Append(')');
                    }
                }
            }
            if (first)
            {
                words.Append($@"(\b(?:{NOMATCH})");
            }
            words.Append(')');
            var numbers = allowNumbers ? $"(\\b{n}\\b)" : NOMATCH;
            var expr = $"{numbers}|{words}";
            _expressions.Add(new ValueAndExpression(value, new Regex(expr.ToString(), RegexOptions.IgnoreCase), maxWords));
            ++n;
            return n;
        }

        private bool ConvertSpecial(object value, object defaultValue, out object newValue)
        {
            bool ok = true;
            newValue = value;
            if (value is Special)
            {
                var special = (Special)value;
                if (special == Special.CurrentChoice && (_noPreference != null || defaultValue != null))
                {
                    newValue = defaultValue;
                }
                else if (special == Special.NoPreference)
                {
                    newValue = null;
                }
                else
                {
                    ok = false;
                }
            }
            return ok;
        }

        private class ValueAndExpression
        {
            public ValueAndExpression(object value, Regex expression, int maxWords)
            {
                Value = value;
                Expression = expression;
                MaxWords = maxWords;
            }

            public readonly object Value;
            public readonly Regex Expression;
            public readonly int MaxWords;
        }

        private readonly IForm<T> _form;
        private readonly string _description;
        private readonly IEnumerable<string> _noPreference;
        private readonly string _currentChoice;
        private readonly bool _allowNumbers;
        private readonly IEnumerable<string> _terms;
        private readonly IEnumerable<object> _values;
        private readonly IEnumerable<DescribeAttribute> _valueDescriptions;
        private readonly DescriptionDelegate _descriptionDelegate;
        private readonly TermsDelegate _termsDelegate;
        private readonly TemplateAttribute _helpFormat;
        private int _max;
        private readonly List<ValueAndExpression> _expressions = new List<ValueAndExpression>();
    }

    /// <summary>
    /// Abstract class for constructing primitive value recognizers.
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    public abstract class RecognizePrimitive<T> : IRecognize<T>
        where T : class
    {

        /// <summary>
        /// Constructor using <see cref="IField{T}"/>.
        /// </summary>
        /// <param name="field">Field to build recognizer for.</param>
        public RecognizePrimitive(IField<T> field)
        {
            _field = field;
            _currentChoices = new HashSet<string>(from choice in field.Form.Configuration.CurrentChoice
                                                  select choice.Trim().ToLower());
            if (field.Optional)
            {
                if (field.IsNullable)
                {
                    _noPreference = new HashSet<string>(from choice in field.Form.Configuration.NoPreference
                                                        select choice.Trim().ToLower());
                }
                else
                {
                    throw new ArgumentException("Primitive values must be nullable to be optional.");
                }
            }
        }

        public virtual object[] PromptArgs()
        {
            return new object[0];
        }

        /// <summary>
        /// Abstract method for parsing input.
        /// </summary>
        /// <param name="input">Input to match.</param>
        /// <returns>TermMatch if input is a match.</returns>
        public abstract TermMatch Parse(string input);

        public virtual IEnumerable<TermMatch> Matches(IMessageActivity input, object defaultValue = null)
        {
            var inputText = MessageActivityHelper.GetSanitizedTextInput(input);

            var matchValue = inputText.Trim().ToLower();
            if (_noPreference != null && _noPreference.Contains(matchValue))
            {
                yield return new TermMatch(0, inputText.Length, 1.0, null);
            }
            else if ((defaultValue != null || _noPreference != null) && (matchValue == string.Empty || _currentChoices.Contains(matchValue)))
            {
                yield return new TermMatch(0, inputText.Length, 1.0, defaultValue);
            }
            else
            {
                var result = Parse(inputText);
                if (result != null)
                {
                    yield return result;
                }
            }
        }

        public abstract IEnumerable<string> ValidInputs(object value);

        public abstract DescribeAttribute ValueDescription(object value);

        public virtual IEnumerable<DescribeAttribute> ValueDescriptions()
        {
            return new DescribeAttribute[0];
        }

        public virtual IEnumerable<object> Values()
        {
            return null;
        }

        public abstract string Help(T state, object defaultValue);

        /// <summary>
        /// Return the help template args for current choice and no preference.
        /// </summary>
        /// <param name="state">Form state.</param>
        /// <param name="defaultValue">Current value of field.</param>
        /// <returns></returns>
        protected List<object> HelpArgs(T state, object defaultValue)
        {
            var args = new List<object>();
            if (defaultValue != null || _field.Optional)
            {
                args.Add(_field.Form.Configuration.CurrentChoice.First());
                if (_field.Optional)
                {
                    args.Add(_field.Form.Configuration.NoPreference.First());
                }
                else
                {
                    args.Add(null);
                }
            }
            else
            {
                args.Add(null);
                args.Add(null);
            }
            return args;
        }

        /// <summary>
        /// Field being filled information.
        /// </summary>
        protected IField<T> _field;

        private HashSet<string> _currentChoices;
        private HashSet<string> _noPreference;
    }

    /// <summary>
    /// Recognize a boolean value.
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    public sealed class RecognizeBool<T> : RecognizePrimitive<T>
        where T : class
    {
        /// <summary>
        /// Construct a boolean recognizer for a field.
        /// </summary>
        /// <param name="field">Boolean field.</param>
        public RecognizeBool(IField<T> field)
            : base(field)
        {
            _yes = new HashSet<string>(from term in field.Form.Configuration.Yes
                                       select term.Trim().ToLower());
            _no = new HashSet<string>(from term in field.Form.Configuration.No
                                      select term.Trim().ToLower());
        }

        public override TermMatch Parse(string input)
        {
            TermMatch result = null;
            var matchValue = input.Trim().ToLower();
            if (_yes.Contains(matchValue))
            {
                result = new TermMatch(0, input.Length, 1.0, true);
            }
            else if (_no.Contains(matchValue))
            {
                result = new TermMatch(0, input.Length, 1.0, false);
            }
            return result;
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.BoolHelp), _field.Form, null);
            var args = HelpArgs(state, defaultValue);
            return prompt.Prompt(state, _field, args.ToArray()).Prompt;
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            return (bool)value
                ? _field.Form.Configuration.Yes
                : _field.Form.Configuration.No;
        }

        public override DescribeAttribute ValueDescription(object value)
        {
            return new DescribeAttribute(((bool)value
                ? _field.Form.Configuration.Yes
                : _field.Form.Configuration.No).First());
        }

        public override IEnumerable<DescribeAttribute> ValueDescriptions()
        {
            return new DescribeAttribute[] { ValueDescription(true), ValueDescription(false) };
        }

        private HashSet<string> _yes;
        private HashSet<string> _no;
    }

    /// <summary>
    /// Recognize a string field.
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    public sealed class RecognizeString<T> : RecognizePrimitive<T>
        where T : class
    {
        /// <summary>
        /// Construct a string recognizer for a field.
        /// </summary>
        /// <param name="field">string field.</param>
        public RecognizeString(IField<T> field)
            : base(field)
        {
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            yield return (string)value;
        }

        public override DescribeAttribute ValueDescription(object value)
        {
            return new DescribeAttribute((string)value);
        }

        public override TermMatch Parse(string input)
        {
            TermMatch result = null;
            if (!string.IsNullOrWhiteSpace(input))
            {
                // Confidence is 0.0 so commands get a crack
                result = new TermMatch(0, input.Length, 0.0, input);
            }
            return result;
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.StringHelp), _field.Form, null);
            var args = HelpArgs(state, defaultValue);
            return prompt.Prompt(state, _field, args.ToArray()).Prompt;
        }
    }

    /// <summary>
    /// Recognize a numeric field.
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    public sealed class RecognizeNumber<T> : RecognizePrimitive<T>
        where T : class
    {
        /// <summary>
        /// Construct a numeric recognizer for a field.
        /// </summary>
        /// <param name="field">Numeric field.</param>
        public RecognizeNumber(IField<T> field)
            : base(field)
        {
            double min, max;
            _showLimits = field.Limits(out min, out max);
            _min = (long)min;
            _max = (long)max;
        }

        public override object[] PromptArgs()
        {
            return _showLimits ? new object[] { _min, _max } : new object[] { null, null };
        }

        public override DescribeAttribute ValueDescription(object value)
        {
            return new DescribeAttribute(((long)Convert.ChangeType(value, typeof(long))).ToString(Thread.CurrentThread.CurrentUICulture.NumberFormat));
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            yield return ((long)value).ToString(Thread.CurrentThread.CurrentUICulture.NumberFormat);
        }

        public override TermMatch Parse(string input)
        {
            TermMatch result = null;
            long number;
            if (long.TryParse(input, NumberStyles.Integer, Thread.CurrentThread.CurrentUICulture.NumberFormat, out number))
            {
                if (number >= _min && number <= _max)
                {
                    result = new TermMatch(0, input.Length, 1.0, number);
                }
            }
            return result;
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.IntegerHelp), _field.Form, null);
            var args = HelpArgs(state, defaultValue);
            if (_showLimits)
            {
                args.Add(_min);
                args.Add(_max);
            }
            return prompt.Prompt(state, _field, args.ToArray()).Prompt;
        }

        private long _min;
        private long _max;
        private bool _showLimits;
    }

    /// <summary>
    /// Recognize a double or float field.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RecognizeDouble<T> : RecognizePrimitive<T>
        where T : class
    {

        /// <summary>
        /// Construct a double or float recognizer for a field.
        /// </summary>
        /// <param name="field">Float or double field.</param>
        public RecognizeDouble(IField<T> field)
            : base(field)
        {
            _showLimits = field.Limits(out _min, out _max);
        }

        public override object[] PromptArgs()
        {
            return _showLimits ? new object[] { _min, _max } : new object[] { null, null };
        }

        public override DescribeAttribute ValueDescription(object value)
        {
            return new DescribeAttribute(((double)Convert.ChangeType(value, typeof(double))).ToString(Thread.CurrentThread.CurrentUICulture.NumberFormat));
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            yield return ((double)value).ToString(Thread.CurrentThread.CurrentUICulture.NumberFormat);
        }

        public override TermMatch Parse(string input)
        {
            TermMatch result = null;
            double number;
            if (double.TryParse(input, NumberStyles.Float, Thread.CurrentThread.CurrentUICulture.NumberFormat, out number))
            {
                if (number >= _min && number <= _max)
                {
                    result = new TermMatch(0, input.Length, 1.0, number);
                }
            }
            return result;
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.DoubleHelp), _field.Form, null);
            var args = HelpArgs(state, defaultValue);
            if (_showLimits)
            {
                args.Add(_min);
                args.Add(_max);
            }
            return prompt.Prompt(state, _field, args.ToArray()).Prompt;
        }

        private double _min;
        private double _max;
        private bool _showLimits;
    }

    /// <summary>
    /// Recognize a date/time expression.
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    /// <remarks>
    /// Expressions recognized are based the C# Chronic parser for English and
    /// the C# DateTime parser otherwise.
    /// </remarks>
    public sealed class RecognizeDateTime<T> : RecognizePrimitive<T>
        where T : class
    {
        /// <summary>
        /// Construct a date/time recognizer.
        /// </summary>
        /// <param name="field">DateTime field.</param>
        public RecognizeDateTime(IField<T> field)
            : base(field)
        {
            _parser = new Chronic.Parser();
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.DateTimeHelp), _field.Form, null);
            var args = HelpArgs(state, defaultValue);
            return prompt.Prompt(state, _field, args.ToArray()).Prompt;
        }

        public override TermMatch Parse(string input)
        {
            TermMatch match = null;
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName != "en")
            {
                DateTime dt;
                if (DateTime.TryParse(input, Thread.CurrentThread.CurrentUICulture.DateTimeFormat, DateTimeStyles.None, out dt))
                {
                    match = new TermMatch(0, input.Length, 1.0, dt);
                }
            }
            else
            {
                var parse = _parser.Parse(input);
                if (parse != null && parse.Start.HasValue)
                {
                    match = new TermMatch(0, input.Length, 1.0, parse.Start.Value);
                }
            }
            return match;
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            yield return ValueDescription(value).Description;
        }

        public override DescribeAttribute ValueDescription(object value)
        {
            return new DescribeAttribute(((DateTime)value).ToString(Thread.CurrentThread.CurrentUICulture.DateTimeFormat));
        }

        private Parser _parser;
    }
}
