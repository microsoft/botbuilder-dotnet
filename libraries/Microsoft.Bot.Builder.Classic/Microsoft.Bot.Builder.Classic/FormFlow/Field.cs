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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    #region Documentation
    /// <summary>   Define field delegate. </summary>
    /// <typeparam name="T">    Form state type. </typeparam>
    /// <param name="state">    Form state. </param>
    /// <param name="field">Field being dynamically defined.</param>
    /// <returns>True if field is defined.</returns>
    /// <remarks>Delegate for dynamically defining a field prompt and recognizer.  You can make use of the fluent methods
    ///          on <see cref="Field{T}"/> to change the characteristics of the field.</remarks>
    #endregion
    public delegate Task<bool> DefineAsyncDelegate<T>(T state, Field<T> field)
        where T : class;

    /// <summary>
    /// A delegate for deciding on the next step in the form to execute.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="value">Value just entered for field.</param>
    /// <param name="state">Current state object.</param>
    /// <returns></returns>
    public delegate NextStep NextDelegate<T>(object value, T state)
        where T : class;

    /// <summary>Base class with declarative implementation of IField. </summary>
    /// <typeparam name="T">Underlying form state.</typeparam>
    public class Field<T> : IField<T>
        where T : class
    {
        /// <summary>   Construct field. </summary>
        /// <param name="name"> Name of field. </param>
        /// <param name="role"> Role field plays in form. </param>
        public Field(string name, FieldRole role)
        {
            _name = name;
            _role = role;
            _min = -double.MaxValue;
            _max = double.MaxValue;
            _limited = false;
        }

        #region IField

        public string Name { get { return _name; } }

        public virtual IForm<T> Form
        {
            get { return this._form; }
            set
            {
                _form = value;
                foreach (var template in _form.Configuration.Templates)
                {
                    if (!_templates.ContainsKey(template.Usage))
                    {
                        AddTemplate(template);
                    }
                }
                if (_define == null)
                {
                    DefinePrompt();
                    DefineRecognizer();
                }
            }
        }

        #region IFieldState
        public virtual object GetValue(T state)
        {
            throw new NotImplementedException();
        }

        public virtual void SetValue(T state, object value)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsUnknown(T state)
        {
            throw new NotImplementedException();
        }

        public virtual void SetUnknown(T state)
        {
            throw new NotImplementedException();
        }

        public virtual Type Type
        {
            get { return _type; }
        }

        public virtual bool Optional
        {
            get
            {
                return _optional;
            }
        }

        public virtual bool IsNullable
        {
            get
            {
                return _isNullable;
            }
        }

        public virtual bool Limits(out double min, out double max)
        {
            min = _min;
            max = _max;
            return _limited;
        }

        public virtual string Pattern
        {
            get { return _pattern; }
        }

        public virtual IEnumerable<string> Dependencies
        {
            get
            {
                return _dependencies;
            }
        }
        #endregion

        #region IFieldDescription
        public virtual FieldRole Role
        {
            get
            {
                return _role;
            }
        }

        public virtual DescribeAttribute FieldDescription
        {
            get
            {
                return _description;
            }
        }

        public virtual IEnumerable<string> FieldTerms
        {
            get
            {
                return _terms.Alternatives;
            }
        }

        public virtual IEnumerable<string> Terms(object value)
        {
            return _valueTerms[value].Alternatives;
        }

        public virtual DescribeAttribute ValueDescription(object value)
        {
            return _valueDescriptions[value];
        }

        public virtual IEnumerable<DescribeAttribute> ValueDescriptions
        {
            get
            {
                return _valueDescriptions.Values;
            }
        }

        public virtual IEnumerable<object> Values
        {
            get
            {
                return _valueDescriptions.Keys;
            }
        }

        public virtual bool AllowsMultiple
        {
            get
            {
                return _allowsMultiple;
            }
        }

        public virtual bool AllowDefault
        {
            get
            {
                return _promptDefinition.AllowDefault != BoolDefault.False;
            }
        }

        public bool AllowNumbers
        {
            get
            {
                return _promptDefinition.AllowNumbers;
            }
        }
        #endregion

        #region IFieldResources

        public virtual void SaveResources()
        {
            var localizer = _form.Resources;
            if (_description.IsLocalizable)
            {
                localizer.Add(_name + nameof(_description), _description.Description);
                localizer.Add(_name + nameof(_description.Image), _description.Image);
                localizer.Add(_name + nameof(_description.Title), _description.Title);
                localizer.Add(_name + nameof(_description.SubTitle), _description.SubTitle);
                localizer.Add(_name + nameof(_description.Message), _description.Message);
            }
            if (_terms.IsLocalizable)
            {
                localizer.Add(_name + nameof(_terms), _terms.Alternatives);
            }
            localizer.Add(_name + nameof(_valueDescriptions), _valueDescriptions);
            localizer.Add(_name + nameof(_valueTerms), _valueTerms);
            if (_promptDefinition != null && _promptDefinition.IsLocalizable)
            {
                localizer.Add(_name + nameof(_promptDefinition), _promptDefinition.Patterns);
            }
            localizer.Add(_name, _templates);
        }

        public virtual void Localize()
        {
            var localizer = _form.Resources;
            string strValue;
            string[] terms;
            if (localizer.Lookup(_name + nameof(_description), out strValue))
            {
                _description.Description = strValue;
            }
            if (localizer.Lookup(_name + nameof(_description.Image), out strValue))
            {
                _description.Image = strValue;
            }
            if (localizer.Lookup(_name + nameof(_description.Title), out strValue))
            {
                _description.Title = strValue;
            }
            if (localizer.Lookup(_name + nameof(_description.SubTitle), out strValue))
            {
                _description.SubTitle = strValue;
            }
            if (localizer.Lookup(_name + nameof(_description.Message), out strValue))
            {
                _description.Message = strValue;
            }
            if (localizer.LookupValues(_name + nameof(_terms), out terms))
            {
                _terms = new TermsAttribute(terms);
            }
            localizer.LookupDictionary(_name + nameof(_valueDescriptions), _valueDescriptions);
            localizer.LookupDictionary(_name + nameof(_valueTerms), _valueTerms);
            string[] patterns;
            if (localizer.LookupValues(_name + nameof(_promptDefinition), out patterns))
            {
                _promptDefinition.Patterns = patterns;
            }
            localizer.LookupTemplates(_name, _templates);
            if (!_promptSet)
            {
                _promptDefinition = null;
            }
            _prompt = null;
            _recognizer = null;
            if (_define == null)
            {
                DefinePrompt();
                DefineRecognizer();
            }
        }

        #endregion

        #region IFieldPrompt

        public virtual bool Active(T state)
        {
            return _condition(state);
        }

        public virtual TemplateAttribute Template(TemplateUsage usage)
        {
            TemplateAttribute template;
            _templates.TryGetValue(usage, out template);
            if (template != null)
            {
                template.ApplyDefaults(_form.Configuration.DefaultPrompt);
            }
            return template;
        }

        public virtual IPrompt<T> Prompt
        {
            get
            {
                return _prompt;
            }
        }

        public async virtual Task<bool> DefineAsync(T state)
        {
            bool result = true;
            if (_define != null)
            {
                if (!_promptSet)
                {
                    _promptDefinition = null;
                }
                _recognizer = null;
                _help = null;
                _prompt = null;
                result = await _define(state, this);
                DefinePrompt();
                DefineRecognizer();
            }
            return result;
        }

        public async virtual Task<ValidateResult> ValidateAsync(T state, object value)
        {
            return await _validate(state, value);
        }

        public virtual IPrompt<T> Help
        {
            get
            {
                return _help;
            }
        }

        public virtual NextStep Next(object value, T state)
        {
            return _next(value, state);
        }

        #endregion
        #endregion

        #region Publics
        /// <summary>Set the field description. </summary>
        /// <param name="description">Field description. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetFieldDescription(string description)
        {
            _description = new DescribeAttribute(description);
            return this;
        }

        /// <summary>
        /// Set the full field description.
        /// </summary>
        /// <param name="description">The field description.</param>
        /// <returns>A <see cref="Field{T}"/>. </returns>
        public Field<T> SetFieldDescription(DescribeAttribute description)
        {
            _description = description;
            return this;
        }

        /// <summary>   Set the terms associated with the field. </summary>
        /// <param name="terms">    The terms. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetFieldTerms(params string[] terms)
        {
            _terms = new TermsAttribute(terms);
            return this;
        }

        /// <summary>   Adds a description for a value. </summary>
        /// <param name="value">        The value. </param>
        /// <param name="description">  Description of the value. </param>
        /// <param name="image">Image to use for value as button.</param>
        /// <param name="message">Message to return when button is pressed.</param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> AddDescription(object value, string description, string image = null, string message = null)
        {
            _valueDescriptions[value] = new DescribeAttribute(description, image, message);
            return this;
        }

        /// <summary>   Adds a full description for a value. </summary>
        /// <param name="value">        The value. </param>
        /// <param name="description">  Description of the value. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> AddDescription(object value, DescribeAttribute description)
        {
            _valueDescriptions[value] = description;
            return this;
        }

        /// <summary>   Adds terms for a value. </summary>
        /// <param name="value">    The value. </param>
        /// <param name="terms">    The terms. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> AddTerms(object value, params string[] terms)
        {
            _valueTerms[value] = new TermsAttribute(terms);
            return this;
        }

        /// <summary>   Adds terms for a value. </summary>
        /// <param name="value">    The value. </param>
        /// <param name="terms">    The terms to add. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> AddTerms(object value, TermsAttribute terms)
        {
            _valueTerms[value] = terms;
            return this;
        }

        /// <summary>   Removes the description and terms associated with a value. </summary>
        /// <param name="value">    The value to remove. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> RemoveValue(object value)
        {
            _valueDescriptions.Remove(value);
            _valueTerms.Remove(value);
            return this;
        }

        /// <summary>   Removes all values and their associated descriptions and terms. </summary>
        /// <returns>   A <see cref="Field{T}"/>.</returns>
        public Field<T> RemoveValues()
        {
            _valueDescriptions.Clear();
            _valueTerms.Clear();
            return this;
        }

        /// <summary>   Sets the type of the underlying field state. </summary>
        /// <param name="type"> The field type. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetType(Type type)
        {
            _type = type;
            return this;
        }

        /// <summary>   Set whether or not a field is optional. </summary>
        /// <param name="optional"> True if field is optional. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetOptional(bool optional = true)
        {
            _optional = optional;
            return this;
        }

        #region Documentation
        /// <summary>   Sets whether or not multiple values are allowed. </summary>
        /// <param name="multiple"> True if multiple values are allowed. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        #endregion
        public Field<T> SetAllowsMultiple(bool multiple = true)
        {
            _allowsMultiple = multiple;
            return this;
        }

        /// <summary>   Set whether or not field is nullable. </summary>
        /// <param name="nullable"> True if field is nullable. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetIsNullable(bool nullable = true)
        {
            _isNullable = nullable;
            return this;
        }

        #region Documentation
        /// <summary>   Define a delegate for checking state to see if field applies. </summary>
        /// <param name="condition">    The condition delegate. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        #endregion
        public Field<T> SetActive(ActiveDelegate<T> condition)
        {
            if (condition != null) _condition = condition;
            return this;
        }

        #region Documentation
        /// <summary>   Define a delegate for dynamically defining field. </summary>
        /// <param name="definition">   The definition delegate. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        /// <remarks>When you dynamically define a field through this delegate you can use all of the fluent methods
        ///          defined on <see cref="Field{T}"/> to change the descriptions and terms dynamically.</remarks>
        #endregion
        public Field<T> SetDefine(DefineAsyncDelegate<T> definition)
        {
            if (definition != null) _define = definition;
            return this;
        }

        /// <summary>   Sets the field prompt. </summary>
        /// <param name="prompt">   The prompt. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetPrompt(PromptAttribute prompt)
        {
            _promptDefinition = prompt;
            _promptSet = true;
            return this;
        }

        /// <summary> Sets the recognizer for the field. </summary>
        /// <param name="recognizer">   The recognizer for the field. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        /// <remarks>
        /// This should only be called when you are dynamically defining a field using a <see cref="DefineAsyncDelegate{T}"/> because
        /// recognizers usually require the field and often change if the localization changes.
        /// </remarks>
        public Field<T> SetRecognizer(IRecognize<T> recognizer)
        {
            _recognizer = recognizer;
            _buildPrompts = true;
            return this;
        }

        /// <summary>   Replace a template in the field. </summary>
        /// <param name="template"> The template. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> ReplaceTemplate(TemplateAttribute template)
        {
            AddTemplate(template);
            return this;
        }

        /// <summary>   Set the field validation. </summary>
        /// <param name="validate"> The validator. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetValidate(ValidateAsyncDelegate<T> validate)
        {
            if (validate != null) _validate = validate;
            return this;
        }

        /// <summary>   Set numeric limits. </summary>
        /// <param name="min">  The minimum. </param>
        /// <param name="max">  The maximum. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetLimits(double min, double max)
        {
            SetLimits(min, max, true);
            return this;
        }

        /// <summary>
        /// Regular expression for validating strings.
        /// </summary>
        /// <param name="pattern">Validation regular expression.</param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetPattern(string pattern)
        {
            _pattern = pattern;
            var regex = new Regex(pattern, RegexOptions.Compiled);
            _validate = async (T state, object value) =>
            {
                var result = new ValidateResult { Value = value };
                if (value == null)
                {
                    result.IsValid = _optional;
                }
                else
                {
                    var match = regex.Match((string)value);
                    result.IsValid = match.Success;
                }
                if (!result.IsValid)
                {
                    result.Feedback = new Prompter<T>(Template(TemplateUsage.NotUnderstood), _form, null).Prompt(state, this, value).Prompt;
                }
                return result;
            };
            return this;
        }

        #region Documentation
        /// <summary>   Define the fields this field depends on. </summary>
        /// <param name="dependencies"> A variable-length parameters list containing dependencies. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        #endregion
        public Field<T> SetDependencies(params string[] dependencies)
        {
            _dependencies = dependencies;
            return this;
        }

        /// <summary>
        /// Delegate for deciding on the next form step to execute.
        /// </summary>
        /// <returns>A <see cref="Field{T}"/>.</returns>
        public Field<T> SetNext(NextDelegate<T> next)
        {
            _next = next;
            return this;
        }

        #endregion

        #region Internals
        protected void DefinePrompt()
        {
            if (_promptDefinition == null)
            {
                TemplateUsage usage = TemplateUsage.None;
                if (_type == null || _type.IsEnum)
                {
                    usage = _allowsMultiple ? TemplateUsage.EnumSelectMany : TemplateUsage.EnumSelectOne;
                }
                else if (_type == typeof(string))
                {
                    usage = TemplateUsage.String;
                }
                else if (_type.IsIntegral())
                {
                    usage = TemplateUsage.Integer;
                }
                else if (_type == typeof(bool))
                {
                    usage = TemplateUsage.Bool;
                }
                else if (_type.IsDouble())
                {
                    usage = TemplateUsage.Double;
                }
                else if (_type == typeof(DateTime))
                {
                    usage = TemplateUsage.DateTime;
                }
                else
                {
                    throw new ArgumentException($"{_name} is not a type FormFlow understands.");
                }
                if (usage != TemplateUsage.None)
                {
                    _promptDefinition = new PromptAttribute(Template(usage));
                }
                _promptSet = false;
            }
            _promptDefinition.ApplyDefaults(_form.Configuration.DefaultPrompt);
        }

        protected void DefineRecognizer()
        {
            if (_recognizer == null)
            {
                if (_type == null || _type.IsEnum)
                {
                    _recognizer = new RecognizeEnumeration<T>(this);
                }
                else if (_type == typeof(bool))
                {
                    _recognizer = new RecognizeBool<T>(this);
                }
                else if (_type == typeof(string))
                {
                    _recognizer = new RecognizeString<T>(this);
                }
                else if (_type.IsIntegral())
                {
                    _recognizer = new RecognizeNumber<T>(this);
                }
                else if (_type.IsDouble())
                {
                    _recognizer = new RecognizeDouble<T>(this);
                }
                else if (_type == typeof(DateTime))
                {
                    _recognizer = new RecognizeDateTime<T>(this);
                }
                else if (_type.IsIEnumerable())
                {
                    var elt = _type.GetGenericElementType();
                    if (elt.IsEnum)
                    {
                        _recognizer = new RecognizeEnumeration<T>(this);
                    }
                }
                _buildPrompts = true;
            }
            if (_buildPrompts)
            {
                var template = Template(TemplateUsage.Help);
                _help = new Prompter<T>(template, _form, _recognizer);
                var prompt = _promptDefinition;
                _prompt = new Prompter<T>(_promptDefinition, _form, _recognizer);
                _buildPrompts = false;
            }
        }

        protected void SetLimits(double min, double max, bool limited)
        {
            _min = min;
            _max = max;
            _limited = limited;
        }

        protected void AddTemplate(TemplateAttribute template)
        {
            _templates[template.Usage] = template;
        }

        protected IForm<T> _form;
        protected string _name;
        protected FieldRole _role;
        protected ActiveDelegate<T> _condition = new ActiveDelegate<T>((state) => true);
        protected DefineAsyncDelegate<T> _define = null;
        protected ValidateAsyncDelegate<T> _validate = new ValidateAsyncDelegate<T>(async (state, value) => new ValidateResult { IsValid = true, Value = value });
        protected NextDelegate<T> _next = new NextDelegate<T>((value, state) => new NextStep());
        protected double _min, _max;
        protected bool _limited;
        protected string _pattern;
        protected string[] _dependencies = Array.Empty<string>();
        protected bool _allowsMultiple;
        protected Type _type;
        protected bool _optional;
        protected bool _isNullable;
        protected bool _keepZero;
        protected DescribeAttribute _description = new DescribeAttribute(null);
        protected TermsAttribute _terms = new TermsAttribute();
        protected Dictionary<object, DescribeAttribute> _valueDescriptions = new Dictionary<object, DescribeAttribute>();
        protected Dictionary<object, TermsAttribute> _valueTerms = new Dictionary<object, TermsAttribute>();
        protected Dictionary<TemplateUsage, TemplateAttribute> _templates = new Dictionary<TemplateUsage, TemplateAttribute>();
        protected bool _promptSet;
        protected PromptAttribute _promptDefinition;
        protected bool _buildPrompts = true;
        protected IRecognize<T> _recognizer;
        protected IPrompt<T> _help;
        protected IPrompt<T> _prompt;
        #endregion
    }

    /// <summary>
    /// Dictionary of all fields indexed by name.
    /// </summary>
    /// <typeparam name="T">Underlying form state.</typeparam>
    public class Fields<T> : IFields<T>
        where T : class
    {
        public IField<T> Field(string name)
        {
            IField<T> field;
            _fields.TryGetValue(name, out field);
            return field;
        }

        public void Add(IField<T> field)
        {
            _fields[field.Name] = field;
        }

        public IEnumerator<IField<T>> GetEnumerator()
        {
            return (from entry in _fields select entry.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (from entry in _fields select entry.Value).GetEnumerator();
        }

        /// <summary>   Mapping from field name to field definition. </summary>
        protected Dictionary<string, IField<T>> _fields = new Dictionary<string, IField<T>>();
    }
}
