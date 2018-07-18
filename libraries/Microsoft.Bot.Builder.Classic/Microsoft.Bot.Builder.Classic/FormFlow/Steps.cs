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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    internal class FieldStep<T> : IStep<T>
        where T : class
    {
        public FieldStep(string name, IForm<T> form)
        {
            _name = name;
            _field = form.Fields.Field(name);
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public StepType Type
        {
            get
            {
                return StepType.Field;
            }
        }

        public TemplateBaseAttribute Annotation
        {
            get
            {
                return _field.Prompt?.Annotation;
            }
        }

        public IField<T> Field
        {
            get
            {
                return _field;
            }
        }

        public void SaveResources()
        {
            _field.SaveResources();
        }

        public void Localize()
        {
            _field.Localize();
        }

        public bool Active(T state)
        {
            return _field.Active(state);
        }

        public async Task<bool> DefineAsync(T state)
        {
            return await _field.DefineAsync(state);
        }

        public FormPrompt Start(IDialogContext context, T state, FormState form)
        {
            form.SetPhase(StepPhase.Responding);
            form.StepState = new FieldStepState(FieldStepStates.SentPrompt);
            return _field.Prompt.Prompt(state, _field, _field.Prompt.Recognizer.PromptArgs());
        }

        public IEnumerable<TermMatch> Match(IDialogContext context, T state, FormState form, IMessageActivity input)
        {
            IEnumerable<TermMatch> matches = null;
            Debug.Assert(form.Phase() == StepPhase.Responding);
            var stepState = (FieldStepState)form.StepState;
            if (stepState.State == FieldStepStates.SentPrompt)
            {
                matches = _field.Prompt.Recognizer.Matches(input, _field.GetValue(state));
            }
            else if (stepState.State == FieldStepStates.SentClarify)
            {
                var fieldState = (FieldStepState)form.StepState;
                var iprompt = _field.Prompt;
                var choiceRecognizer = ClarifyRecognizer(fieldState, iprompt.Recognizer);
                matches = MatchAnalyzer.Coalesce(MatchAnalyzer.HighestConfidence(choiceRecognizer.Matches(input)), MessageActivityHelper.GetSanitizedTextInput(input)).ToArray();
                if (matches.Count() > 1)
                {
                    matches = new TermMatch[0];
                }
            }
#if DEBUG
            if (FormDialog.DebugRecognizers)
            {
                MatchAnalyzer.PrintMatches(matches, 2);
            }
#endif
            return matches;
        }

        public async Task<StepResult> ProcessAsync(IDialogContext context, T state, FormState form, IMessageActivity input, IEnumerable<TermMatch> matches)
        {
            var inputText = MessageActivityHelper.GetSanitizedTextInput(input);

            ValidateResult feedback = new ValidateResult();
            feedback.IsValid = true;
            feedback.Feedback = null;
            feedback.FeedbackCard = null;
            feedback.Choices = null;
            FormPrompt prompt = null;
            FormPrompt feedbackPrompt = null;
            var iprompt = _field.Prompt;
            var fieldState = (FieldStepState)form.StepState;
            object response = null;
            if (fieldState.State == FieldStepStates.SentPrompt)
            {
                // Response to prompt
                var firstMatch = matches.FirstOrDefault();
                if (matches.Count() == 1)
                {
                    response = firstMatch.Value;
                    if (_field.AllowsMultiple && response != null
                        && (response.GetType() == typeof(string) || !response.GetType().IsIEnumerable()))
                    {
                        response = new List<object>() { response };
                    }
                    feedback = await SetValueAsync(state, response, form);
                    if (!feedback.IsValid && feedback.Choices != null)
                    {
                        var choices = new Ambiguous(inputText.Substring(firstMatch.Start, firstMatch.Length), feedback.Choices);
                        fieldState.State = FieldStepStates.SentClarify;
                        fieldState.Settled = new List<object>();
                        fieldState.Clarifications = new List<Ambiguous>() { choices };
                        response = SetValue(state, null);
                        prompt = ClarifyPrompt((FieldStepState)form.StepState, iprompt.Recognizer, state);
                    }
                }
                else if (matches.Count() > 1)
                {
                    // Check multiple matches for ambiguity
                    var groups = MatchAnalyzer.GroupedMatches(matches);
                    // 1) Could be multiple match groups like for ingredients.
                    // 2) Could be overlapping matches like "onion".
                    // 3) Could be multiple matches where only one is expected.

                    if (!_field.AllowsMultiple)
                    {
                        // Create a single group of all possibilities if only want one value
                        var mergedGroup = groups.SelectMany((group) => group).ToList();
                        groups = new List<List<TermMatch>>() { mergedGroup };
                    }
                    var ambiguous = new List<Ambiguous>();
                    var settled = new List<object>();
                    foreach (var choices in groups)
                    {
                        if (choices.Count > 1)
                        {
                            var unclearResponses = string.Join(" ", (from choice in choices select inputText.Substring(choice.Start, choice.Length)).Distinct());
                            var values = from match in choices select match.Value;
                            ambiguous.Add(new Ambiguous(unclearResponses, values));
                        }
                        else
                        {
                            var matchValue = choices.First().Value;
                            if (matchValue != null && matchValue.GetType() != typeof(string) && matchValue.GetType().IsIEnumerable())
                            {
                                foreach (var value in (System.Collections.IEnumerable)matchValue)
                                {
                                    settled.Add(value);
                                }
                            }
                            else
                            {
                                settled.Add(choices.First().Value);
                            }
                        }
                    }
                    if (settled.Count > 1)
                    {
                        // Remove no preference if present
                        settled.Remove(null);
                    }

                    if (ambiguous.Count > 0)
                    {
                        // Need 1 or more clarifications
                        fieldState.State = FieldStepStates.SentClarify;
                        fieldState.Settled = settled;
                        fieldState.Clarifications = ambiguous;
                        response = SetValue(state, null);
                        prompt = ClarifyPrompt((FieldStepState)form.StepState, iprompt.Recognizer, state);
                    }
                    else
                    {
                        if (_field.AllowsMultiple)
                        {
                            response = settled;
                            feedback = await SetValueAsync(state, response, form);
                        }
                        else
                        {
                            Debug.Assert(settled.Count == 1);
                            response = settled.First();
                            feedback = await SetValueAsync(state, response, form);
                        }
                    }
                }
                var unmatched = MatchAnalyzer.Unmatched(inputText, matches);
                var unmatchedWords = string.Join(" ", unmatched);
                var nonNoise = Language.NonNoiseWords(Language.WordBreak(unmatchedWords)).ToArray();
                fieldState.Unmatched = null;
                if (_field.Prompt.Annotation.Feedback == FeedbackOptions.Always)
                {
                    fieldState.Unmatched = string.Join(" ", nonNoise);
                }
                else if (_field.Prompt.Annotation.Feedback == FeedbackOptions.Auto
                        && nonNoise.Any()
                        && unmatched.Any())
                {
                    fieldState.Unmatched = string.Join(" ", nonNoise);
                }
            }
            else if (fieldState.State == FieldStepStates.SentClarify)
            {
                if (matches.Count() == 1)
                {
                    // Clarified ambiguity
                    var clarify = NeedsClarification(fieldState);
                    fieldState.Settled.Add(matches.First().Value);
                    fieldState.Clarifications.Remove(clarify);
                    if (prompt == null)
                    {
                        // No clarification left, so set the field
                        if (_field.AllowsMultiple)
                        {
                            response = fieldState.Settled;
                            feedback = await SetValueAsync(state, response, form);
                        }
                        else
                        {
                            Debug.Assert(fieldState.Settled.Count == 1);
                            response = fieldState.Settled.First();
                            feedback = await SetValueAsync(state, response, form);
                        }
                        form.SetPhase(StepPhase.Completed);
                    }
                }
            }
            if (form.Phase() == StepPhase.Completed)
            {
                form.StepState = null;
                if (fieldState.Unmatched != null)
                {
                    if (feedback.FeedbackCard != null)
                    {
                        feedbackPrompt = feedback.FeedbackCard;
                    }
                    else if (feedback.Feedback != null)
                    {
                        feedbackPrompt = new FormPrompt { Prompt = feedback.Feedback };
                    }
                    else
                    {
                        if (fieldState.Unmatched != string.Empty)
                        {
                            feedbackPrompt = new Prompter<T>(_field.Template(TemplateUsage.Feedback), _field.Form, null).Prompt(state, _field, fieldState.Unmatched);
                        }
                        else
                        {
                            feedbackPrompt = new Prompter<T>(_field.Template(TemplateUsage.Feedback), _field.Form, null).Prompt(state, _field);
                        }
                    }
                }
            }
            var next = _field.Next(response, state);
            return new StepResult(feedback.IsValid, next, feedbackPrompt ?? (feedback.FeedbackCard ?? new FormPrompt { Prompt = feedback.Feedback }), prompt);
        }

        public bool InClarify(FormState form)
        {
            var state = (FieldStepState)form.StepState;
            return state.State == FieldStepStates.SentClarify;
        }

        public FormPrompt NotUnderstood(IDialogContext context, T state, FormState form, IMessageActivity input)
        {
            var inputText = MessageActivityHelper.GetSanitizedTextInput(input);

            FormPrompt feedback = null;
            var iprompt = _field.Prompt;
            var fieldState = (FieldStepState)form.StepState;
            if (fieldState.State == FieldStepStates.SentPrompt)
            {
                feedback = Template(TemplateUsage.NotUnderstood).Prompt(state, _field, inputText);
            }
            else if (fieldState.State == FieldStepStates.SentClarify)
            {
                feedback = Template(TemplateUsage.NotUnderstood, ClarifyRecognizer(fieldState, _field.Prompt.Recognizer)).Prompt(state, _field, inputText);
            }
            return feedback;
        }

        public bool Back(IDialogContext context, T state, FormState form)
        {
            bool backedUp = false;
            var fieldState = (FieldStepState)form.StepState;
            if (fieldState.State == FieldStepStates.SentClarify)
            {
                var desc = _field.Form.Fields.Field(_name);
                if (desc.AllowsMultiple)
                {
                    desc.SetValue(state, fieldState.Settled);
                }
                else if (fieldState.Settled.Any())
                {
                    desc.SetValue(state, fieldState.Settled.First());
                }
                form.SetPhase(StepPhase.Ready);
                backedUp = true;
            }
            return backedUp;
        }

        public FormPrompt Help(T state, FormState form, string commandHelp)
        {
            var fieldState = (FieldStepState)form.StepState;
            IPrompt<T> template;
            if (fieldState.State == FieldStepStates.SentClarify)
            {
                template = Template(TemplateUsage.HelpClarify, ClarifyRecognizer(fieldState, _field.Prompt.Recognizer));
            }
            else
            {
                template = Template(TemplateUsage.Help, _field.Prompt.Recognizer);
            }
            var help = template.Prompt(state, _field, "* " + template.Recognizer.Help(state, _field.GetValue(state)), commandHelp);
            return new FormPrompt { Prompt = "* " + help.Prompt, Buttons = help.Buttons };
        }

        public IEnumerable<string> Dependencies
        {
            get
            {
                return Array.Empty<string>();
            }
        }

        private IPrompt<T> Template(TemplateUsage usage, IRecognize<T> recognizer = null)
        {
            var template = _field.Template(usage);
            return new Prompter<T>(template, _field.Form, recognizer == null ? _field.Prompt.Recognizer : recognizer);
        }

        private object SetValue(T state, object value)
        {
            var desc = _field.Form.Fields.Field(_name);
            if (value == null)
            {
                desc.SetUnknown(state);
            }
            else
            {
                desc.SetValue(state, value);
            }
            return value;
        }

        private async Task<ValidateResult> SetValueAsync(T state, object value, FormState form)
        {
            var desc = _field.Form.Fields.Field(_name);
            var feedback = await desc.ValidateAsync(state, value);
            if (feedback.IsValid)
            {
                SetValue(state, feedback.Value);
                form.SetPhase(StepPhase.Completed);
            }
            else if (feedback.Feedback == null)
            {
                feedback.Feedback = string.Empty;
            }
            return feedback;
        }

        private Ambiguous NeedsClarification(FieldStepState stepState)
        {
            Ambiguous clarify = null;
            foreach (var clarification in stepState.Clarifications)
            {
                if (clarification.Values.Length > 1)
                {
                    clarify = clarification;
                    break;
                }
            }
            return clarify;
        }

        private class FieldClarify : Field<T>
        {
            public FieldClarify(IField<T> root)
                : base(root.Name, FieldRole.Value)
            {
                Form = root.Form;
                var template = root.Template(TemplateUsage.Clarify);
                SetPrompt(new PromptAttribute(template));
                ReplaceTemplate(template);
                ReplaceTemplate(root.Template(template.AllowNumbers ? TemplateUsage.EnumOneNumberHelp : TemplateUsage.EnumManyNumberHelp));
            }

            public override bool IsUnknown(T state)
            {
                return true;
            }
        }

        private IField<T> ClarifyField(Ambiguous clarify, IRecognize<T> recognizer)
        {
            var field = new FieldClarify(_field);
            foreach (var value in clarify.Values)
            {
                var choice = value as Choice;
                if (choice != null)
                {
                    field.AddDescription(choice.Value,
                        choice.Description.Description,
                        choice.Description.Image,
                        choice.Description.Message);
                    field.AddTerms(choice.Value, choice.Terms.Alternatives);
                }
                else
                {
                    var desc = recognizer.ValueDescription(value);
                    field.AddDescription(value, desc.Description, desc.Image);
                    field.AddTerms(value, recognizer.ValidInputs(value).ToArray());
                }
            }
            return field;
        }

        private IRecognize<T> ClarifyRecognizer(FieldStepState stepState, IRecognize<T> recognizer)
        {
            var clarify = NeedsClarification(stepState);
            return (clarify != null ? new RecognizeEnumeration<T>(ClarifyField(clarify, recognizer)) : null);
        }

        private FormPrompt ClarifyPrompt(FieldStepState stepState, IRecognize<T> recognizer, T state)
        {
            var clarify = NeedsClarification(stepState);
            FormPrompt prompt = null;
            if (clarify != null)
            {
                var field = ClarifyField(clarify, recognizer);
                var prompter = new Prompter<T>(field.Template(TemplateUsage.Clarify), field.Form, new RecognizeEnumeration<T>(field));
                prompt = prompter.Prompt(state, field, clarify.Response);
            }
            return prompt;
        }

        internal enum FieldStepStates { Unknown, SentPrompt, SentClarify };

        [Serializable]
        internal class Ambiguous
        {
            public readonly string Response;
            public object[] Values;
            public Ambiguous(string response, IEnumerable<object> values)
            {
                Response = response;
                Values = values.ToArray<object>();
            }
        }

        [Serializable]
        internal class FieldStepState
        {
            internal FieldStepStates State;
            internal string Unmatched;
            internal List<object> Settled;
            internal List<Ambiguous> Clarifications;
            public FieldStepState(FieldStepStates state)
            {
                State = state;
            }
        }

        private readonly string _name;
        private readonly IField<T> _field;
    }

    internal class ConfirmStep<T> : IStep<T>
        where T : class
    {
        public ConfirmStep(IField<T> field)
        {
            _field = field;
        }

        public bool Back(IDialogContext context, T state, FormState form)
        {
            return false;
        }

        public IField<T> Field
        {
            get
            {
                return _field;
            }
        }

        public void SaveResources()
        {
            _field.SaveResources();
        }

        public void Localize()
        {
            _field.Localize();
        }

        public bool Active(T state)
        {
            return _field.Active(state);
        }

        public IEnumerable<TermMatch> Match(IDialogContext context, T state, FormState form, IMessageActivity input)
        {
            return _field.Prompt.Recognizer.Matches(input);
        }

        public string Name
        {
            get
            {
                return _field.Name;
            }
        }

        public TemplateBaseAttribute Annotation
        {
            get
            {
                return _field.Prompt?.Annotation;
            }
        }

        public FormPrompt NotUnderstood(IDialogContext context, T state, FormState form, IMessageActivity input)
        {
            var template = _field.Template(TemplateUsage.NotUnderstood);
            var prompter = new Prompter<T>(template, _field.Form, null);
            return prompter.Prompt(state, _field, MessageActivityHelper.GetSanitizedTextInput(input));
        }

        public async Task<StepResult> ProcessAsync(IDialogContext context, T state, FormState form, IMessageActivity input, IEnumerable<TermMatch> matches)
        {
            var value = matches.First().Value;
            form.StepState = null;
            form.SetPhase((bool)value ? StepPhase.Completed : StepPhase.Ready);
            var next = _field.Next(value, state);
            return new StepResult(true, next, feedback: null, prompt: null);
        }

        public async Task<bool> DefineAsync(T state)
        {
            return await _field.DefineAsync(state);
        }

        public FormPrompt Start(IDialogContext context, T state, FormState form)
        {
            form.SetPhase(StepPhase.Responding);
            return _field.Prompt.Prompt(state, _field);
        }

        public FormPrompt Help(T state, FormState form, string commandHelp)
        {
            var template = _field.Template(TemplateUsage.HelpConfirm);
            var prompt = new Prompter<T>(template, _field.Form, _field.Prompt.Recognizer);
            var help = prompt.Prompt(state, _field, "* " + prompt.Recognizer.Help(state, null), commandHelp);
            return new FormPrompt { Prompt = "* " + help.Prompt, Buttons = help.Buttons };
        }

        public bool InClarify(FormState form)
        {
            return false;
        }

        public StepType Type
        {
            get
            {
                return StepType.Confirm;
            }
        }

        public IEnumerable<string> Dependencies
        {
            get
            {
                return _field.Dependencies;
            }
        }

        private readonly IField<T> _field;
    }

    internal class NavigationField<T> : Field<T>
        where T : class
    {
        public NavigationField(string name, string startField, IForm<T> form, T state, FormState formState, Fields<T> fields)
            : base(name, FieldRole.Value)
        {
            Form = form;
            var field = form.Fields.Field(startField);
            SetFieldDescription(_form.Configuration.Navigation);
            SetOptional();
            foreach (var value in formState.Next.Names)
            {
                var svalue = (string)value;
                var sfield = form.Fields.Field(svalue);
                var fieldPrompt = sfield.Template(TemplateUsage.NavigationFormat);
                var prompter = new Prompter<T>(fieldPrompt, form, sfield.Prompt.Recognizer);
                AddDescription(value, prompter.Prompt(state, sfield).Prompt, null, sfield.FieldDescription.Message ?? sfield.FieldDescription.Description);
                AddTerms(value, form.Fields.Field(svalue).FieldTerms.ToArray());
            }
            var template = field.Template(TemplateUsage.Navigation);
            SetPrompt(new PromptAttribute(template));
            SetRecognizer(new RecognizeEnumeration<T>(this));
            _prompt = new Prompter<T>(template, form, _recognizer, fields);
        }

        public override bool IsUnknown(T state)
        {
            return true;
        }
    }

    internal class NavigationStep<T> : IStep<T>
        where T : class
    {
        private const string _name = "__navigate__";
        public NavigationStep(string startField, IForm<T> form, T state, FormState formState)
        {
            var fields = new Fields<T>();
            _field = new NavigationField<T>(_name, startField, form, state, formState, fields);
            fields.Add(_field);
            _fields = fields;
        }

        public bool Back(IDialogContext context, T state, FormState form)
        {
            form.Next = null;
            return false;
        }

        public IField<T> Field
        {
            get
            {
                return _field;
            }
        }

        public bool Active(T state)
        {
            return true;
        }

        public IEnumerable<TermMatch> Match(IDialogContext context, T state, FormState form, IMessageActivity input)
        {
            return _field.Prompt.Recognizer.Matches(input);
        }

        public string Name
        {
            get
            {
                return "Navigation";
            }
        }

        public TemplateBaseAttribute Annotation
        {
            get
            {
                return _field.Prompt.Annotation;
            }
        }

        public bool InClarify(FormState form)
        {
            return false;
        }

        public FormPrompt NotUnderstood(IDialogContext context, T state, FormState form, IMessageActivity input)
        {
            var template = _field.Template(TemplateUsage.NotUnderstood);
            return new Prompter<T>(template, _field.Form, _field.Prompt.Recognizer, _fields).Prompt(state, _field, MessageActivityHelper.GetSanitizedTextInput(input));
        }

        public async Task<StepResult> ProcessAsync(IDialogContext context, T state, FormState form, IMessageActivity input, IEnumerable<TermMatch> matches)
        {
            NextStep next;
            form.Next = null;
            var val = matches.First().Value;
            if (val == null)
            {
                next = new NextStep();
            }
            else
            {
                next = new NextStep(new string[] { (string)val });
            }
            return new StepResult(true, next, feedback: null, prompt: null);
        }

        public Task<bool> DefineAsync(T state)
        {
            throw new NotImplementedException();
        }

        public FormPrompt Start(IDialogContext context, T state, FormState form)
        {
            return _field.Prompt.Prompt(state, _field);
        }

        public StepType Type
        {
            get
            {
                return StepType.Navigation;
            }
        }

        public FormPrompt Help(T state, FormState form, string commandHelp)
        {
            var recognizer = _field.Prompt.Recognizer;
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.HelpNavigation), _field.Form, recognizer, _fields);
            var help = prompt.Prompt(state, _field, "* " + recognizer.Help(state, null), commandHelp);
            return new FormPrompt { Prompt = "* " + help.Prompt, Buttons = help.Buttons };
        }

        public void SaveResources()
        {
        }

        public void Localize()
        {
        }

        public IEnumerable<string> Dependencies
        {
            get
            {
                return Array.Empty<string>();
            }
        }

        private readonly IField<T> _field;
        private readonly IFields<T> _fields;
    }

    internal class MessageStep<T> : IStep<T>
        where T : class
    {
        public MessageStep(MessageDelegate<T> generateMessage, ActiveDelegate<T> condition, IEnumerable<string> dependencies, IForm<T> form)
        {
            _name = "message" + form.Steps.Count.ToString();
            _form = form;
            _message = generateMessage;
            _condition = (condition == null ? (state) => true : condition);
            _dependencies = dependencies ?? form.Dependencies(form.Steps.Count);
        }

        public MessageStep(PromptAttribute prompt, ActiveDelegate<T> condition, IEnumerable<string> dependencies, IForm<T> form)
        {
            _name = "message" + form.Steps.Count.ToString();
            _form = form;
            _promptDefinition = prompt;
            _condition = (condition == null ? (state) => true : condition);
            _dependencies = dependencies ?? form.Dependencies(form.Steps.Count);
        }

        public bool Active(T state)
        {
            return _condition(state);
        }

        public bool Back(IDialogContext context, T state, FormState form)
        {
            return false;
        }

        public FormPrompt Help(T state, FormState form, string commandHelp)
        {
            return null;
        }

        public IEnumerable<string> Dependencies
        {
            get
            {
                return _dependencies;
            }
        }

        public IField<T> Field
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<TermMatch> Match(IDialogContext context, T state, FormState form, IMessageActivity input)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public TemplateBaseAttribute Annotation
        {
            get { return _promptDefinition; }
        }

        public bool InClarify(FormState form)
        {
            return false;
        }

        public FormPrompt NotUnderstood(IDialogContext context, T state, FormState form, IMessageActivity input)
        {
            throw new NotImplementedException();
        }

        public Task<StepResult> ProcessAsync(IDialogContext context, T state, FormState form, IMessageActivity input, IEnumerable<TermMatch> matches)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DefineAsync(T state)
        {
            if (_message != null)
            {
                _promptDefinition = await _message(state);
            }
            return true;
        }

        public FormPrompt Start(IDialogContext context, T state, FormState form)
        {
            form.SetPhase(StepPhase.Completed);
            var prompt = new Prompter<T>(_promptDefinition, _form, null);
            return prompt.Prompt(state, null);
        }

        public void SaveResources()
        {
            if (_message == null)
            {
                _form.Resources.Add(_name, _promptDefinition.Patterns);
            }
        }

        public void Localize()
        {
            if (_message == null)
            {
                string[] patterns;
                _form.Resources.LookupValues(_name, out patterns);
                if (patterns != null) _promptDefinition.Patterns = patterns;
            }
        }

        public StepType Type
        {
            get
            {
                return StepType.Message;
            }
        }

        private readonly string _name;
        private readonly IForm<T> _form;
        private PromptAttribute _promptDefinition;
        private readonly MessageDelegate<T> _message;
        private readonly ActiveDelegate<T> _condition;
        private readonly IEnumerable<string> _dependencies;
    }
}
