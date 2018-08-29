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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Luis.Models;

namespace Microsoft.Bot.Builder.Classic.FormFlow
{
    /// <summary>
    /// Static factory methods for creating form dialogs.
    /// </summary>
    public static class FormDialog
    {
        /// <summary>
        /// Create an <see cref="IFormDialog{T}"/> using the default <see cref="BuildFormDelegate{T}"/>.
        /// </summary>
        /// <typeparam name="T">The form type.</typeparam>
        /// <param name="options">The form options.</param>
        /// <returns>The form dialog.</returns>
        public static IFormDialog<T> FromType<T>(FormOptions options = FormOptions.None) where T : class, new()
        {
            return new FormDialog<T>(new T(), null, options);
        }

        /// <summary>
        /// Create an <see cref="IFormDialog{T}"/> using the <see cref="BuildFormDelegate{T}"/> parameter.
        /// </summary>
        /// <typeparam name="T">The form type.</typeparam>
        /// <param name="buildForm">The delegate to build the form.</param>
        /// <param name="options">The form options.</param>
        /// <returns>The form dialog.</returns>
        public static IFormDialog<T> FromForm<T>(BuildFormDelegate<T> buildForm, FormOptions options = FormOptions.None) where T : class, new()
        {
            return new FormDialog<T>(new T(), buildForm, options);
        }


        #region IForm<T> statics
#if DEBUG
        internal static bool DebugRecognizers = false;
#endif
        #endregion
    }

    /// <summary>
    /// Options for form execution.
    /// </summary>
    [Flags]
    public enum FormOptions
    {
        /// <summary>
        /// No options.
        /// </summary>
        None,

        /// <summary>
        /// Prompt when the dialog starts.
        /// </summary>
        PromptInStart,

        /// <summary>  
        /// Prompt for fields that already have a value in the initial state when processing form.
        /// </summary>
        PromptFieldsWithValues
    };

    /// <summary>
    /// Delegate for building the form.
    /// </summary>
    /// <typeparam name="T">The form state type.</typeparam>
    /// <returns>An <see cref="IForm{T}"/>.</returns>
    /// <remarks>This is a delegate so that we can rebuild the form and don't have to serialize
    /// the form definition with every message.</remarks>
    public delegate IForm<T> BuildFormDelegate<T>() where T : class;

    /// <summary>
    /// Form dialog to fill in your state.
    /// </summary>
    /// <typeparam name="T">The type to fill in.</typeparam>
    /// <remarks>
    /// This is the root class for managing a FormFlow dialog. It is usually created
    /// through the factory methods <see cref="FormDialog.FromForm{T}(BuildFormDelegate{T}, FormOptions)"/>
    /// or <see cref="FormDialog.FromType{T}"/>. 
    /// </remarks>
    [Serializable]
    public sealed class FormDialog<T> : IFormDialog<T>, ISerializable
        where T : class
    {
        // constructor arguments
        private readonly T _state;
        private readonly BuildFormDelegate<T> _buildForm;
        private readonly IEnumerable<EntityModel> _entities;
        private readonly FormOptions _options;

        // instantiated in constructor, saved when serialized
        private readonly FormState _formState;

        // instantiated in constructor, re-instantiated when deserialized
        private readonly IForm<T> _form;
        private readonly IField<T> _commands;

        public T State => _state;

        private static IForm<T> BuildDefaultForm()
        {
            return new FormBuilder<T>().AddRemainingFields().Build();
        }

        #region Documentation
        /// <summary>   Constructor for creating a FormFlow dialog. </summary>
        /// <param name="state">        The initial state. </param>
        /// <param name="buildForm">    A delegate for building the form. </param>
        /// <param name="options">      Options for controlling the form. </param>
        /// <param name="entities">     Optional entities to process into the form. </param>
        /// <param name="cultureInfo">  The culture to use. </param>
        /// <remarks>For building forms <see cref="IFormBuilder{T}"/>.</remarks>
        #endregion
        public FormDialog(T state, BuildFormDelegate<T> buildForm = null, FormOptions options = FormOptions.None, IEnumerable<EntityModel> entities = null, CultureInfo cultureInfo = null)
        {
            buildForm = buildForm ?? BuildDefaultForm;
            entities = entities ?? Enumerable.Empty<EntityModel>();
            if (cultureInfo != null)
            {
                CultureInfo.CurrentUICulture = cultureInfo;
                CultureInfo.CurrentCulture = cultureInfo;
            }

            // constructor arguments
            SetField.NotNull(out this._state, nameof(state), state);
            SetField.NotNull(out this._buildForm, nameof(buildForm), buildForm);
            SetField.NotNull(out this._entities, nameof(entities), entities);
            this._options = options;

            // make our form
            var form = _buildForm();

            // instantiated in constructor, saved when serialized
            this._formState = new FormState(form.Steps.Count);

            // instantiated in constructor, re-instantiated when deserialized
            this._form = form;
            this._commands = this._form.BuildCommandRecognizer();
        }

        private FormDialog(SerializationInfo info, StreamingContext context)
        {
            // constructor arguments
            SetField.NotNullFrom(out this._state, nameof(this._state), info);
            SetField.NotNullFrom(out this._buildForm, nameof(this._buildForm), info);
            SetField.NotNullFrom(out this._entities, nameof(this._entities), info);
            this._options = info.GetValue<FormOptions>(nameof(this._options));

            // instantiated in constructor, saved when serialized
            SetField.NotNullFrom(out this._formState, nameof(this._formState), info);

            // instantiated in constructor, re-instantiated when deserialized
            this._form = _buildForm();
            this._commands = this._form.BuildCommandRecognizer();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // constructor arguments
            info.AddValue(nameof(this._state), this._state);
            info.AddValue(nameof(this._buildForm), this._buildForm);
            info.AddValue(nameof(this._entities), this._entities);
            info.AddValue(nameof(this._options), this._options);

            // instantiated in constructor, saved when serialized
            info.AddValue(nameof(this._formState), this._formState);
        }

        #region IFormDialog<T> implementation

        IForm<T> IFormDialog<T>.Form { get { return this._form; } }

        #endregion

        #region IDialog implementation
        async Task IDialog<T>.StartAsync(IDialogContext context)
        {
            if (this._entities.Any())
            {
                var inputs = new List<Tuple<int, string>>();
                var entityGroups = (from entity in this._entities group entity by entity.Role ?? entity.Type);
                foreach (var entityGroup in entityGroups)
                {
                    var step = _form.Step(entityGroup.Key);
                    if (step != null)
                    {
                        var builder = new StringBuilder();
                        var first = true;
                        foreach (var entity in entityGroup)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                builder.Append(' ');
                            }
                            builder.Append(entity.Entity);
                        }
                        inputs.Add(Tuple.Create(_form.StepIndex(step), builder.ToString()));
                    }
                }
                if (inputs.Any())
                {
                    // Descending because last entry is first processed
                    _formState.FieldInputs = (from input in inputs orderby input.Item1 descending select input).ToList();
                }
            }
            await SkipSteps();
            _formState.Step = 0;
            _formState.StepState = null;

            if (this._options.HasFlag(FormOptions.PromptInStart))
            {
                await MessageReceived(context, null);
            }
            else
            {
                context.Wait(MessageReceived);
            }
        }

        public async Task MessageReceived(IDialogContext context, IAwaitable<Schema.IMessageActivity> toBot)
        {
            try
            {
                var message = toBot == null ? null : await toBot;

                // Ensure we have initial definition for field steps
                foreach (var step in _form.Steps)
                {
                    if (step.Type == StepType.Field && step.Field.Prompt == null)
                    {
                        await step.DefineAsync(_state);
                    }
                }

                var next = (_formState.Next == null ? new NextStep() : ActiveSteps(_formState.Next, _state));
                bool waitForMessage = false;
                FormPrompt lastPrompt = _formState.LastPrompt;

                Func<FormPrompt, IStep<T>, Task<FormPrompt>> PostAsync = async (prompt, step) =>
                {
                    return await _form.Prompt(context, prompt, _state, step.Field);
                };

                Func<IStep<T>, IEnumerable<TermMatch>, Task<bool>> DoStepAsync = async (step, matches) =>
                {
                    var result = await step.ProcessAsync(context, _state, _formState, message, matches);
                    await SkipSteps();
                    next = result.Next;
                    if (result.Feedback?.Prompt != null)
                    {
                        await PostAsync(result.Feedback, step);
                        if (_formState.Phase() != StepPhase.Completed)
                        {
                            if (!_formState.ProcessInputs)
                            {
                                await PostAsync(lastPrompt, step);
                                waitForMessage = true;
                            }
                            else if (result.Prompt?.Buttons != null)
                            {
                                // We showed buttons so allow them to be pressed
                                waitForMessage = true;
                            }
                            else
                            {
                                // After simple feedback, reset to ready
                                _formState.SetPhase(StepPhase.Ready);
                            }
                        }
                    }

                    if (result.Prompt != null)
                    {
                        lastPrompt = await PostAsync(result.Prompt, step);
                        waitForMessage = true;
                    }

                    return true;
                };

                while (!waitForMessage && MoveToNext(next))
                {
                    IStep<T> step = null;
                    IEnumerable<TermMatch> matches = null;
                    if (next.Direction == StepDirection.Named && next.Names.Length > 1)
                    {
                        // We need to choose between multiple next steps
                        bool start = (_formState.Next == null);
                        _formState.Next = next;
                        step = new NavigationStep<T>(_form.Steps[_formState.Step].Name, _form, _state, _formState);
                        if (start)
                        {
                            lastPrompt = await PostAsync(step.Start(context, _state, _formState), step);
                            waitForMessage = true;
                        }
                        else
                        {
                            // Responding
                            matches = step.Match(context, _state, _formState, message);
                        }
                    }
                    else
                    {
                        // Processing current step
                        step = _form.Steps[_formState.Step];
                        if (await step.DefineAsync(_state))
                        {
                            if (_formState.Phase() == StepPhase.Ready)
                            {
                                if (step.Type == StepType.Message)
                                {
                                    await PostAsync(step.Start(context, _state, _formState), step);
                                    next = new NextStep();
                                }
                                else if (_formState.ProcessInputs)
                                {
                                    message = MessageActivityHelper.BuildMessageWithText(_formState.FieldInputs.Last().Item2);
                                    lastPrompt = step.Start(context, _state, _formState);
                                }
                                else
                                {
                                    lastPrompt = await PostAsync(step.Start(context, _state, _formState), step);
                                    waitForMessage = true;
                                }
                            }
                            else if (_formState.Phase() == StepPhase.Responding)
                            {
                                matches = step.Match(context, _state, _formState, message);
                            }
                        }
                        else
                        {
                            _formState.SetPhase(StepPhase.Completed);
                            lastPrompt = null;
                            next = new NextStep(StepDirection.Next);
                        }
                    }

                    if (matches != null)
                    {
                        var inputText = MessageActivityHelper.GetSanitizedTextInput(message);
                        matches = MatchAnalyzer.Coalesce(matches, inputText).ToArray();
                        if (MatchAnalyzer.IsFullMatch(inputText, matches))
                        {
                            await DoStepAsync(step, matches);
                        }
                        else
                        {
                            // Filter non-active steps out of command matches
                            var messageText = message.Text;
                            var commands =
                                (messageText == null || messageText.Trim().StartsWith("\""))
                                ? new TermMatch[0]
                                : (from command in MatchAnalyzer.Coalesce(_commands.Prompt.Recognizer.Matches(message), messageText)
                                   where (command.Value is FormCommand
                                          || (!_formState.ProcessInputs && _form.Fields.Field((string)command.Value).Active(_state)))
                                   select command).ToArray();

                            if (commands.Length == 1 && MatchAnalyzer.IsFullMatch(messageText, commands))
                            {
                                FormPrompt feedback;
                                next = DoCommand(context, _state, _formState, step, commands, out feedback);
                                if (feedback != null)
                                {
                                    await PostAsync(feedback, step);
                                    await PostAsync(lastPrompt, step);
                                    waitForMessage = true;
                                }
                            }
                            else
                            {
                                if (matches.Count() == 0 && commands.Count() == 0)
                                {
                                    await PostAsync(step.NotUnderstood(context, _state, _formState, message), step);
                                    if (_formState.ProcessInputs && !step.InClarify(_formState))
                                    {
                                        _formState.SetPhase(StepPhase.Ready);
                                    }
                                    else
                                    {
                                        waitForMessage = true;
                                    }
                                }
                                else
                                {
                                    // Go with response since it looks possible
                                    var bestMatch = MatchAnalyzer.BestMatches(matches, commands);
                                    if (bestMatch == 0)
                                    {
                                        await DoStepAsync(step, matches);
                                    }
                                    else
                                    {
                                        FormPrompt feedback;
                                        next = DoCommand(context, _state, _formState, step, commands, out feedback);
                                        if (feedback != null)
                                        {
                                            await PostAsync(feedback, step);
                                            await PostAsync(lastPrompt, step);
                                            waitForMessage = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    next = ActiveSteps(next, _state);
                }

                if (next.Direction == StepDirection.Complete || next.Direction == StepDirection.Quit)
                {
                    if (next.Direction == StepDirection.Complete)
                    {
                        if (_form.Completion != null)
                        {
                            await _form.Completion(context, _state);
                        }
                        context.Done(_state);
                    }
                    else if (next.Direction == StepDirection.Quit)
                    {
                        throw new FormCanceledException<T>("Form quit.")
                        {
                            LastForm = _state,
                            Last = _form.Steps[_formState.Step].Name,
                            Completed = (from step in _form.Steps
                                         where _formState.Phase(_form.StepIndex(step)) == StepPhase.Completed
                                         select step.Name).ToArray()
                        };
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    _formState.LastPrompt = (FormPrompt)lastPrompt?.Clone();
                    context.Wait(MessageReceived);
                }
            }
            catch (Exception inner)
            {
                if (!(inner is FormCanceledException<T>))
                {
                    throw new FormCanceledException<T>(inner.Message, inner)
                    {
                        LastForm = _state,
                        Last = _form.Steps[_formState.Step].Name,
                        Completed = (from step in _form.Steps
                                     where _formState.Phase(_form.StepIndex(step)) == StepPhase.Completed
                                     select step.Name).ToArray()
                    };
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion

        #region Implementation

        private async Task SkipSteps()
        {
            if (!_options.HasFlag(FormOptions.PromptFieldsWithValues))
            {
                // Skip steps that already have a value if they are nullable and valid.
                foreach (var step in _form.Steps)
                {
                    int stepi = _form.StepIndex(step);
                    if (step.Type == StepType.Field
                        && _formState.Phase(stepi) == StepPhase.Ready
                        && !step.Field.IsUnknown(_state)
                        && step.Field.IsNullable)
                    {
                        var defined = await step.DefineAsync(_state);
                        if (defined)
                        {
                            var val = step.Field.GetValue(_state);
                            var result = await step.Field.ValidateAsync(_state, val);
                            if (result.IsValid)
                            {
                                bool ok = true;
                                double min, max;
                                if (step.Field.Limits(out min, out max))
                                {
                                    var num = (double)Convert.ChangeType(val, typeof(double));
                                    ok = (num >= min && num <= max);
                                }
                                if (ok)
                                {
                                    _formState.SetPhase(stepi, StepPhase.Completed);
                                }
                            }
                        }
                    }
                }
            }
        }

        private NextStep ActiveSteps(NextStep next, T state)
        {
            var result = next;
            if (next.Direction == StepDirection.Named)
            {
                var names = (from name in next.Names where _form.Fields.Field(name).Active(state) select name);
                var count = names.Count();
                if (count == 0)
                {
                    result = new NextStep();
                }
                else if (count != result.Names.Length)
                {
                    result = new NextStep(names);
                }
            }
            return result;
        }

        /// <summary>
        /// Find the next step to execute.
        /// </summary>
        /// <param name="next">What step to execute next.</param>
        /// <returns>True if can switch to step.</returns>
        private bool MoveToNext(NextStep next)
        {
            bool found = false;
            switch (next.Direction)
            {
                case StepDirection.Complete:
                    break;
                case StepDirection.Named:
                    _formState.StepState = null;
                    if (next.Names.Length == 0)
                    {
                        goto case StepDirection.Next;
                    }
                    else if (next.Names.Length == 1)
                    {
                        var name = next.Names.First();
                        var nextStep = -1;
                        for (var i = 0; i < _form.Steps.Count(); ++i)
                        {
                            if (_form.Steps[i].Name == name)
                            {
                                nextStep = i;
                                break;
                            }
                        }
                        if (nextStep == -1)
                        {
                            throw new ArgumentOutOfRangeException("NextStep", "Does not correspond to a field in the form.");
                        }
                        if (_form.Steps[nextStep].Active(_state))
                        {
                            var current = _form.Steps[_formState.Step];
                            _formState.SetPhase(_form.Fields.Field(current.Name).IsUnknown(_state) ? StepPhase.Ready : StepPhase.Completed);
                            _formState.History.Push(_formState.Step);
                            _formState.Step = nextStep;
                            _formState.SetPhase(StepPhase.Ready);
                            found = true;
                        }
                        else
                        {
                            // If we went to a state which is not active fall through to the next active if any
                            goto case StepDirection.Next;
                        }
                    }
                    else
                    {
                        // Always mark multiple names as found so we can handle the user navigation
                        found = true;
                    }
                    break;
                case StepDirection.Next:
                    {
                        var start = _formState.Step;
                        // Reset any non-optional field step that has been reset to no value
                        for (var i = 0; i < _form.Steps.Count; ++i)
                        {
                            var step = _form.Steps[i];
                            if (step.Type == StepType.Field && _formState.Phase(i) == StepPhase.Completed && !step.Field.Optional && step.Field.IsUnknown(_state))
                            {
                                _formState.SetPhase(i, StepPhase.Ready);
                            }
                        }
                        // Next ready step including current one
                        for (var offset = 0; offset < _form.Steps.Count; ++offset)
                        {
                            var istep = (start + offset) % _form.Steps.Count;
                            var step = _form.Steps[istep];
                            _formState.Step = istep;
                            if (offset > 0)
                            {
                                _formState.StepState = null;
                                _formState.Next = null;
                            }
                            if ((_formState.Phase(istep) == StepPhase.Ready || _formState.Phase(istep) == StepPhase.Responding)
                                && step.Active(_state))
                            {
                                // Ensure all dependencies have values
                                foreach (var dependency in step.Dependencies)
                                {
                                    var dstep = _form.Step(dependency);
                                    var dstepi = _form.StepIndex(dstep);
                                    if (dstep.Active(_state) && _formState.Phases[dstepi] != StepPhase.Completed)
                                    {
                                        _formState.Step = dstepi;
                                        break;
                                    }
                                }
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            next.Direction = StepDirection.Complete;
                        }
                        else
                        {
                            var normalStep = _formState.Step;
                            // Process initial messages first, then FieldInputs
                            if ((_formState.ProcessInputs || _form.Steps[normalStep].Type != StepType.Message) && _formState.FieldInputs != null)
                            {
                                // Override normal choice with FieldInputs
                                Func<bool> NextFieldInput = () =>
                                {
                                    var foundInput = false;
                                    while (_formState.FieldInputs.Any() && !foundInput)
                                    {
                                        var possible = _formState.FieldInputs.Last().Item1;
                                        if (_form.Steps[possible].Active(_state))
                                        {
                                            _formState.Step = possible;
                                            foundInput = true;
                                        }
                                        else
                                        {
                                            _formState.FieldInputs.Pop();
                                        }
                                    }
                                    if (!_formState.FieldInputs.Any())
                                    {
                                        if (_options.HasFlag(FormOptions.PromptFieldsWithValues))
                                        {
                                            _formState.Reset();
                                        }
                                        else
                                        {
                                            _formState.ProcessInputs = false;
                                            _formState.FieldInputs = null;
                                            _formState.Step = 0;
                                        }
                                        // Skip initial messages since we showed them already
                                        while (_formState.Step < _form.Steps.Count() && _form.Steps[_formState.Step].Type == StepType.Message)
                                        {
                                            _formState.SetPhase(StepPhase.Completed);
                                            ++_formState.Step;
                                        }
                                    }
                                    return foundInput;
                                };
                                if (!_formState.ProcessInputs)
                                {
                                    // Start of processing inputs
                                    _formState.ProcessInputs = NextFieldInput();
                                }
                                else if (_formState.Phase(start) == StepPhase.Completed || _formState.Phase(start) == StepPhase.Ready)
                                {
                                    // Reset state of just completed step
                                    if (_options.HasFlag(FormOptions.PromptFieldsWithValues))
                                    {
                                        _formState.SetPhase(StepPhase.Ready);
                                    }
                                    // Move on to next field input if any
                                    _formState.FieldInputs.Pop();
                                    NextFieldInput();
                                }
                            }
                            else
                            {
                                if (_formState.Step != start && _form.Steps[start].Type != StepType.Message)
                                {
                                    _formState.History.Push(start);
                                }
                            }
                        }
                    }
                    break;
                case StepDirection.Previous:
                    while (_formState.History.Count() > 0)
                    {
                        var lastStepIndex = _formState.History.Pop();
                        var lastStep = _form.Steps[lastStepIndex];
                        if (lastStep.Active(_state))
                        {
                            var step = _form.Steps[_formState.Step];
                            _formState.SetPhase(step.Field.IsUnknown(_state) ? StepPhase.Ready : StepPhase.Completed);
                            _formState.Step = lastStepIndex;
                            _formState.SetPhase(StepPhase.Ready);
                            _formState.StepState = null;
                            _formState.Next = null;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        next.Direction = StepDirection.Quit;
                    }
                    break;
                case StepDirection.Quit:
                    break;
                case StepDirection.Reset:
                    _formState.Reset();
                    // Because we redo phase they can go through everything again but with defaults.
                    found = true;
                    break;
            }
            return found;
        }

        private NextStep DoCommand(IDialogContext context, T state, FormState form, IStep<T> step, IEnumerable<TermMatch> matches, out FormPrompt feedback)
        {
            // TODO: What if there are more than one command?
            feedback = null;
            var next = new NextStep();
            var value = matches.First().Value;
            if (value is FormCommand)
            {
                switch ((FormCommand)value)
                {
                    case FormCommand.Backup:
                        {
                            next.Direction = step.Back(context, state, form) ? StepDirection.Next : StepDirection.Previous;
                        }
                        break;
                    case FormCommand.Help:
                        {
                            var field = step.Field;
                            var builder = new StringBuilder();
                            foreach (var entry in _form.Configuration.Commands)
                            {
                                builder.Append("* ");
                                builder.AppendLine(entry.Value.Help);
                            }
                            var navigation = new Prompter<T>(field.Template(TemplateUsage.NavigationCommandHelp), _form, null);
                            var active = (from istep in _form.Steps
                                          where !form.ProcessInputs && istep.Type == StepType.Field && istep.Active(state)
                                          select istep.Field.FieldDescription.Description).ToArray();
                            if (active.Length > 1)
                            {
                                var activeList = Language.BuildList(active, navigation.Annotation.ChoiceSeparator, navigation.Annotation.ChoiceLastSeparator);
                                builder.Append("* ");
                                builder.Append(navigation.Prompt(state, null, activeList));
                            }
                            feedback = step.Help(state, form, builder.ToString());
                        }
                        break;
                    case FormCommand.Quit: next.Direction = StepDirection.Quit; break;
                    case FormCommand.Reset: next.Direction = StepDirection.Reset; break;
                    case FormCommand.Status:
                        {
                            var prompt = new PromptAttribute("{*}");
                            feedback = new Prompter<T>(prompt, _form, null).Prompt(state, null);
                        }
                        break;
                }
            }
            else
            {
                var name = (string)value;
                var istep = _form.Step(name);
                if (istep != null && istep.Active(state))
                {
                    next = new NextStep(new string[] { name });
                }
            }
            return next;
        }

        #endregion
    }
}

namespace Microsoft.Bot.Builder.Classic.Luis.Models
{
    [Serializable]
    public partial class EntityModel
    {
    }

    [Serializable]
    public partial class IntentRecommendation
    {
    }
}


