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

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.FormFlow
{
    /// <summary>
    /// A delegate for testing a form state to see if a particular step is active.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="state">Form state to test.</param>
    /// <returns>True if step is active given the current form state.</returns>
    public delegate bool ActiveDelegate<T>(T state);

    /// <summary>
    /// Choice for clarifying an ambiguous value in <see cref="ValidateResult"/>.
    /// </summary>
    [Serializable]
    public class Choice
    {
        /// <summary>
        /// Value to return if choice is selected.
        /// </summary>
        public object Value;

        /// <summary>
        /// Description of value.
        /// </summary>
        public DescribeAttribute Description;

        /// <summary>
        /// Terms to match value.
        /// </summary>
        public TermsAttribute Terms;
    }

    /// <summary>   Encapsulates the result of a <see cref="ValidateAsyncDelegate{T}"/> </summary>
    /// <remarks>
    ///          If <see cref="IsValid"/> is true, then the field will be set to <see cref="Value"/>.
    ///          Otherwise if <see cref="Choices"/> is  non-null they will be used to select a clarifying value.
    ///          if <see cref="FeedbackCard"/> is non-null the resulting card will be displayed.
    ///          Otherwise the <see cref="Feedback"/> string will be shown to provide feedback on the value.
    ///          </remarks>
    public class ValidateResult
    {
        /// <summary>   Feedback to provide back to the user on the input. </summary>
        public string Feedback;

        /// <summary>
        /// Fully specified feedback card.
        /// </summary>
        public FormPrompt FeedbackCard;

        /// <summary>   True if value is a valid response. </summary>
        public bool IsValid;

        /// <summary>
        /// Value to put in the field if result is valid.
        /// </summary>
        /// <remarks>This provides an opportunity for validation to compute the final value.</remarks>
        public object Value;

        /// <summary>
        /// Choices for clarifying response.
        /// </summary>
        public IEnumerable<Choice> Choices;
    }

    /// <summary>
    /// A delegate for validating a particular response to a prompt.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="state">Form state to test.</param>
    /// <param name="value">Response value to validate.</param>
    /// <returns><see cref="ValidateResult"/> describing validity, transformed value, feedback or choices for clarification.</returns>
    public delegate Task<ValidateResult> ValidateAsyncDelegate<T>(T state, object value);

    /// <summary>
    /// A delegate called when a form is completed.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="context">Session where form dialog is taking place.</param>
    /// <param name="state">Completed form state.</param>
    /// <remarks>
    /// This delegate gives an opportunity to take an action on a completed form
    /// such as sending it to your service.  It cannot be used to create a new
    /// dialog or return a value to the parent dialog.
    /// </remarks>
    public delegate Task OnCompletionAsyncDelegate<T>(IDialogContext context, T state);

    /// <summary>
    /// Interface for controlling a FormFlow dialog.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <remarks>
    /// <see cref="FormDialog{T}"/> is an implementation of this interface.
    /// </remarks>
    /// <exception cref="FormCanceledException{T}">Thrown when the user quits while filling in a form, or there is an underlying exception in the code.</exception>
    public interface IFormDialog<T> : IDialog<T>
        where T : class
    {
        /// <summary>
        /// The form specification.
        /// </summary>
        IForm<T> Form { get; }
    }

    /// <summary>
    /// Commands supported in form dialogs.
    /// </summary>
    public enum FormCommand
    {
        /// <summary>
        /// Move back to the previous step.
        /// </summary>
        Backup,

        /// <summary>
        /// Ask for help on responding to the current field.
        /// </summary>
        Help,

        /// <summary>
        /// Quit filling in the current form and return failure to parent dialog.
        /// </summary>
        Quit,

        /// <summary>
        /// Reset the status of the form dialog.
        /// </summary>
        Reset,

        /// <summary>
        /// Provide feedback to the user on the current form state.
        /// </summary>
        Status
    };

    /// <summary>
    /// Description of all the information needed for a built-in command.
    /// </summary>
    public class CommandDescription
    {
        /// <summary>
        /// Description of the command.
        /// </summary>
        public string Description;

        /// <summary>
        /// Regexs for matching the command.
        /// </summary>
        public string[] Terms;

        /// <summary>
        /// Help string for the command.
        /// </summary>
        public string Help;

        /// <summary>
        /// Construct the description of a built-in command.
        /// </summary>
        /// <param name="description">Description of the command.</param>
        /// <param name="terms">Terms that match the command.</param>
        /// <param name="help">Help on what the command does.</param>
        public CommandDescription(string description, string[] terms, string help)
        {
            Description = description;
            Terms = terms;
            Help = help;
        }
    }

    #region Documentation
    /// <summary>   Exception generated when form filling is canceled by user quit or exception. </summary>
    /// <remarks>In the case of user quit or an exception the strongly typed exception <see cref="FormCanceledException{T}"/>
    ///          is actually thrown, but this provides simple access to the Last step.</remarks>
    #endregion
    [Serializable]
    public class FormCanceledException : OperationCanceledException
    {
        #region Documentation
        /// <summary>   Constructor with message and inner exception. </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        /// <remarks>In the case of quit by the user, the inner exception will be null.</remarks>
        #endregion
        public FormCanceledException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>   The names of completed steps. </summary>
        public IEnumerable<string> Completed { get; internal set; }

        /// <summary>   Name of the step that quit or threw an exception. </summary>
        public string Last { get; internal set; }
    }

    #region Documentation
    /// <summary>   Exception generated when form filling is canceled by user quit or exception. </summary>
    /// <typeparam name="T">    Underlying form type. </typeparam>
    #endregion
    [Serializable]
    public class FormCanceledException<T> : FormCanceledException
    {
        /// <summary>   Constructor with message and inner exception. </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        /// <remarks>In the case of user quit, the inner exception will be null.</remarks>
        public FormCanceledException(string message, Exception inner = null)
            : base(message, inner)
        {
            LastForm = default(T);
        }

        /// <summary>   Gets the partial form when the user quits or there is an exception. </summary>
        public T LastForm { get; internal set; }
    }
}

