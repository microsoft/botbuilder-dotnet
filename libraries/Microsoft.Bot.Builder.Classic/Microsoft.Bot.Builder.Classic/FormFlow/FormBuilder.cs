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

using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
using Microsoft.Bot.Builder.Classic.Resource;
using Microsoft.Bot.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using Microsoft.Bot.Builder.Classic.Dialogs;
using System.Threading.Tasks;
using System.Text;

namespace Microsoft.Bot.Builder.Classic.FormFlow
{
    #region Documentation
    /// <summary>Abstract base class for Form Builders.</summary>
    /// <typeparam name="T">Form state class. </typeparam>
    #endregion
    public abstract class FormBuilderBase<T> : IFormBuilder<T>
        where T : class
    {
        public virtual IForm<T> Build(Assembly resourceAssembly = null, string resourceName = null)
        {
            if (resourceAssembly == null)
            {
                resourceAssembly = typeof(T).Assembly;
            }
            if (resourceName == null)
            {
                resourceName = typeof(T).FullName;
            }
            if (this._form._prompter == null)
            {
                this._form._prompter = async (context, prompt, state, field) =>
                {
                    var preamble = context.MakeMessage();
                    var promptMessage = context.MakeMessage();
                    if (prompt.GenerateMessages(preamble, promptMessage))
                    {
                        await context.PostAsync(preamble);
                    }
                    await context.PostAsync(promptMessage);
                    return prompt;
                };
            }
            var lang = resourceAssembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>();
            if (lang != null && !string.IsNullOrWhiteSpace(lang.CultureName))
            {
                IEnumerable<string> missing, extra;
                string name = null;
                foreach (var resource in resourceAssembly.GetManifestResourceNames())
                {
                    if (resource.Contains(resourceName))
                    {
                        var pieces = resource.Split('.');
                        name = string.Join(".", pieces.Take(pieces.Count() - 1));
                        break;
                    }
                }
                if (name != null)
                {
                    var rm = new ResourceManager(name, resourceAssembly);
                    var rs = rm.GetResourceSet(Thread.CurrentThread.CurrentUICulture, true, true);
                    _form.Localize(rs.GetEnumerator(), out missing, out extra);
                    if (missing.Any())
                    {
                        throw new MissingManifestResourceException($"Missing resources {missing}");
                    }
                }
            }
            Validate();
            return this._form;
        }

        public FormConfiguration Configuration { get { return _form.Configuration; } }

        public bool HasField(string name)
        {
            return _form.Fields.Field(name) != null;
        }

        public virtual IFormBuilder<T> Message(string message, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            _form._steps.Add(new MessageStep<T>(new PromptAttribute(message), condition, dependencies, _form));
            return this;
        }

        public virtual IFormBuilder<T> Message(PromptAttribute prompt, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            _form._steps.Add(new MessageStep<T>(prompt, condition, dependencies, _form));
            return this;
        }

        public virtual IFormBuilder<T> Message(MessageDelegate<T> generateMessage, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            _form._steps.Add(new MessageStep<T>(generateMessage, condition, dependencies, _form));
            return this;
        }

        public virtual IFormBuilder<T> Field(IField<T> field)
        {
            return AddField(field);
        }

        public virtual IFormBuilder<T> Confirm(string prompt, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            return Confirm(new PromptAttribute(prompt) { ChoiceFormat = Resources.ConfirmChoiceFormat, AllowDefault = BoolDefault.False }, condition, dependencies);
        }

        public virtual IFormBuilder<T> Confirm(PromptAttribute prompt, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            if (condition == null) condition = state => true;
            dependencies = dependencies ?? _form.Dependencies(_form.Steps.Count());
            var confirmation = new Confirmation<T>(prompt, condition, dependencies, _form);
            confirmation.Form = _form;
            _form._fields.Add(confirmation);
            _form._steps.Add(new ConfirmStep<T>(confirmation));
            return this;
        }

        public virtual IFormBuilder<T> Confirm(MessageDelegate<T> generateMessage, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            if (condition == null) condition = state => true;
            dependencies = dependencies ?? _form.Dependencies(_form.Steps.Count());
            var confirmation = new Confirmation<T>(generateMessage, condition, dependencies, _form);
            confirmation.Form = _form;
            _form._fields.Add(confirmation);
            _form._steps.Add(new ConfirmStep<T>(confirmation));
            return this;
        }

        public virtual IFormBuilder<T> OnCompletion(OnCompletionAsyncDelegate<T> callback)
        {
            _form._completion = callback;
            return this;
        }

        public virtual IFormBuilder<T> Prompter(PromptAsyncDelegate<T> prompter)
        {
            _form._prompter = prompter;
            return this;
        }

        public abstract IFormBuilder<T> Field(string name, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null);
        public abstract IFormBuilder<T> Field(string name, string prompt, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null);
        public abstract IFormBuilder<T> Field(string name, PromptAttribute prompt, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null);
        public abstract IFormBuilder<T> AddRemainingFields(IEnumerable<string> exclude = null);

        private Dictionary<TemplateUsage, int> _templateArgs = new Dictionary<TemplateUsage, int>
        {
            {TemplateUsage.Bool, 0 },
            { TemplateUsage.BoolHelp, 1},
            { TemplateUsage.Clarify, 1},
            { TemplateUsage.Confirmation, 0 },
            { TemplateUsage.CurrentChoice, 0},
            { TemplateUsage.DateTime, 0},
            { TemplateUsage.DateTimeHelp, 2},
            { TemplateUsage.Double, 2},
            { TemplateUsage.DoubleHelp, 4},
            { TemplateUsage.EnumManyNumberHelp, 3},
            { TemplateUsage.EnumOneNumberHelp, 3},
            { TemplateUsage.EnumManyWordHelp, 3},
            { TemplateUsage.EnumOneWordHelp, 3},
            { TemplateUsage.EnumSelectOne, 0},
            { TemplateUsage.EnumSelectMany, 0},
            { TemplateUsage.Feedback, 1},
            { TemplateUsage.Help, 2},
            { TemplateUsage.HelpClarify, 2},
            { TemplateUsage.HelpConfirm, 2},
            { TemplateUsage.HelpNavigation, 2},
            { TemplateUsage.Integer, 2},
            { TemplateUsage.IntegerHelp, 4},
            { TemplateUsage.Navigation, 0},
            { TemplateUsage.NavigationCommandHelp, 1},
            { TemplateUsage.NavigationFormat, 0},
            { TemplateUsage.NavigationHelp, 2},
            { TemplateUsage.NoPreference, 0},
            { TemplateUsage.NotUnderstood, 1},
            { TemplateUsage.StatusFormat, 0},
            { TemplateUsage.String, 0},
            { TemplateUsage.StringHelp, 2},
            { TemplateUsage.Unspecified, 0},
        };

        private int TemplateArgs(TemplateUsage usage)
        {
            int args;
            if (!_templateArgs.TryGetValue(usage, out args))
            {
                throw new ArgumentException("Missing template usage for validation");
            }
            return args;
        }

        private void Validate()
        {
            foreach (var step in _form._steps)
            {
                // Validate prompt
                var annotation = step.Annotation;
                if (annotation != null)
                {
                    var name = step.Type == StepType.Field ? step.Name : string.Empty;
                    foreach (var pattern in annotation.Patterns)
                    {
                        ValidatePattern(pattern, _form.Fields.Field(name), 5);
                    }
                    if (step.Type != StepType.Message)
                    {
                        foreach (TemplateUsage usage in Enum.GetValues(typeof(TemplateUsage)))
                        {
                            if (usage != TemplateUsage.None)
                            {
                                foreach (var pattern in step.Field.Template(usage).Patterns)
                                {
                                    ValidatePattern(pattern, _form.Fields.Field(name), TemplateArgs(usage));
                                }
                            }
                        }
                    }
                }
            }
            ValidatePattern(_form.Configuration.DefaultPrompt.ChoiceFormat, null, 2);
        }

        private void ValidatePattern(string pattern, IField<T> field, int maxArgs)
        {
            if (!Prompter<T>.ValidatePattern(_form, pattern, field, maxArgs))
            {
                throw new ArgumentException(string.Format("Illegal pattern: \"{0}\"", pattern));
            }
        }

        private IFormBuilder<T> AddField(IField<T> field)
        {
            field.Form = _form;
            _form._fields.Add(field);
            var step = new FieldStep<T>(field.Name, _form);
            var stepIndex = this._form._steps.FindIndex(s => s.Name == field.Name);
            if (stepIndex >= 0)
            {
                _form._steps[stepIndex] = step;
            }
            else
            {
                _form._steps.Add(step);
            }
            return this;
        }

        protected internal sealed class Form : IForm<T>
        {
            internal readonly FormConfiguration _configuration = new FormConfiguration();
            internal readonly Fields<T> _fields = new Fields<T>();
            internal readonly List<IStep<T>> _steps = new List<IStep<T>>();
            internal OnCompletionAsyncDelegate<T> _completion = null;
            internal PromptAsyncDelegate<T> _prompter = null;
            internal ILocalizer _resources = new Localizer() { Culture = CultureInfo.CurrentUICulture };

            public Form()
            {
            }

            internal override ILocalizer Resources
            {
                get
                {
                    return _resources;
                }
            }

            public override void SaveResources(IResourceWriter writer)
            {
                _resources = new Localizer() { Culture = CultureInfo.CurrentUICulture };
                foreach (var step in _steps)
                {
                    step.SaveResources();
                }
                _resources.Save(writer);
            }

            public override void Localize(IDictionaryEnumerator reader, out IEnumerable<string> missing, out IEnumerable<string> extra)
            {
                foreach (var step in _steps)
                {
                    step.SaveResources();
                }
                _resources = _resources.Load(reader, out missing, out extra);
                foreach (var step in _steps)
                {
                    step.Localize();
                }
            }

            internal override FormConfiguration Configuration
            {
                get
                {
                    return _configuration;
                }
            }

            internal override IReadOnlyList<IStep<T>> Steps
            {
                get
                {
                    return _steps;
                }
            }

            internal override async Task<FormPrompt> Prompt(IDialogContext context, FormPrompt prompt, T state, IField<T> field)
            {
                return prompt == null ? prompt : await _prompter(context, prompt, state, field);
            }

            internal override OnCompletionAsyncDelegate<T> Completion
            {
                get
                {
                    return _completion;
                }
            }

            public override IFields<T> Fields
            {
                get
                {
                    return _fields;
                }
            }
        }

        protected internal Form _form = new Form();
    }

    #region Documentation
    /// <summary>   Build a form by specifying messages, fields and confirmations via reflection or programatically.</summary>
    /// <typeparam name="T">Form state class. </typeparam>
    /// <remarks>
    /// Fields will be defined through reflection over the type <typeparamref name="T"/> and attributes like 
    /// <see cref="DescribeAttribute"/>,
    /// <see cref="NumericAttribute"/>, 
    /// <see cref="OptionalAttribute"/>
    /// <see cref="PatternAttribute"/>, 
    /// <see cref="PromptAttribute"/>, 
    /// <see cref="TermsAttribute"/> and 
    /// <see cref="TemplateAttribute"/>.   
    /// For all of the attributes, reasonable defaults will be generated.
    /// </remarks>
    #endregion
    public sealed class FormBuilder<T> : FormBuilderBase<T>
        where T : class
    {
        private readonly bool _ignoreAnnotations;

        /// <summary>
        /// Create a new form builder for building a form using reflection.
        /// </summary>
        /// <param name="ignoreAnnotations">True to ignore any attributes on the form class.</param>
        public FormBuilder(bool ignoreAnnotations = false)
            : base()
        {
            _ignoreAnnotations = ignoreAnnotations;
        }

        public override IForm<T> Build(Assembly resourceAssembly = null, string resourceName = null)
        {
            if (!_form._steps.Any((step) => step.Type == StepType.Field))
            {
                var paths = new List<string>();
                FieldPaths(typeof(T), string.Empty, paths);
                foreach (var path in paths)
                {
                    Field(new FieldReflector<T>(path, _ignoreAnnotations));
                }
                Confirm(new PromptAttribute(_form.Configuration.Template(TemplateUsage.Confirmation)));
            }
            return base.Build(resourceAssembly, resourceName);
        }

        public override IFormBuilder<T> Field(string name, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null)
        {
            var field = new FieldReflector<T>(name, _ignoreAnnotations);
            field.SetActive(active);
            field.SetValidate(validate);
            return Field(field);
        }

        public override IFormBuilder<T> Field(string name, string prompt, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null)
        {
            return Field(name, new PromptAttribute(prompt), active, validate);
        }

        public override IFormBuilder<T> Field(string name, PromptAttribute prompt, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null)
        {
            var field = new FieldReflector<T>(name, _ignoreAnnotations);
            field.SetActive(active);
            field.SetValidate(validate);
            field.SetPrompt(prompt);
            return Field(field);
        }

        public override IFormBuilder<T> AddRemainingFields(IEnumerable<string> exclude = null)
        {
            var exclusions = (exclude == null ? Array.Empty<string>() : exclude.ToArray());
            var paths = new List<string>();
            FieldPaths(typeof(T), string.Empty, paths);
            foreach (var path in paths)
            {
                if (!exclusions.Contains(path) && !HasField(path))
                {
                    Field(new FieldReflector<T>(path, _ignoreAnnotations));
                }
            }
            return this;
        }

        private void FieldPaths(Type type, string path, List<string> paths)
        {
            var newPath = (path == string.Empty ? path : path + ".");
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => !f.IsDefined(typeof(IgnoreFieldAttribute))))
            {
                TypePaths(field.FieldType, newPath + field.Name, paths);
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead && property.CanWrite)
                {
                    TypePaths(property.PropertyType, newPath + property.Name, paths);
                }
            }
        }

        private void TypePaths(Type type, string path, List<string> paths)
        {
            if (type.IsClass)
            {
                if (type == typeof(string))
                {
                    paths.Add(path);
                }
                else if (type.IsIEnumerable())
                {
                    var elt = type.GetGenericElementType();
                    if (elt.IsEnum)
                    {
                        paths.Add(path);
                    }
                    else
                    {
                        // TODO: What to do about enumerations of things other than enums?
                    }
                }
                else
                {
                    FieldPaths(type, path, paths);
                }
            }
            else if (type.IsEnum)
            {
                paths.Add(path);
            }
            else if (type == typeof(bool))
            {
                paths.Add(path);
            }
            else if (type.IsIntegral())
            {
                paths.Add(path);
            }
            else if (type.IsDouble())
            {
                paths.Add(path);
            }
            else if (type.IsNullable() && type.IsValueType)
            {
                paths.Add(path);
            }
            else if (type == typeof(DateTime))
            {
                paths.Add(path);
            }
        }
    }
}
