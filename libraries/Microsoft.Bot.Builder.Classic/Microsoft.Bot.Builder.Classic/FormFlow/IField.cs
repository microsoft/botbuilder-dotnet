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

using Microsoft.Bot.Builder.Classic.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    /// <summary>
    /// Interface that defines basic access to a field.
    /// </summary>
    /// <typeparam name="T">The form state that is read or written to.</typeparam>
    public interface IFieldState<T>
    {
        /// <summary>
        /// Get this field value from form state.
        /// </summary>
        /// <param name="state">Form state to get field value from.</param>
        /// <returns>Current value found in state.</returns>
        object GetValue(T state);

        /// <summary>
        /// Set this field value in form state.
        /// </summary>
        /// <param name="state">Form state to set field value in.</param>
        /// <param name="value">New value.</param>
        void SetValue(T state, object value);

        /// <summary>
        /// Test to see if the field value form state has a value.
        /// </summary>
        /// <param name="state">Form state to check.</param>
        /// <returns>True if value is unknown.</returns>
        /// <remarks>
        /// For value types (numbers, bools, date time) a value is unknown only if the field is nullable and it is null.
        /// For enum based values (both simple and enumerated) they can also be nullable or the 0 enum value if not nullable.
        /// For non value types like string the test is to see if the field is actually null.
        /// </remarks>
        bool IsUnknown(T state);

        /// <summary>
        /// Set this field value in form state to unknown.
        /// </summary>
        /// <param name="state">Form state with field value to set to unknown.</param>
        /// <remarks>
        /// For value types (numbers, bools, date time) the value is set to null if nullable.
        /// For enum types it is set to null if nullable or 0 if not.  
        /// For non value types like string set the value to null.
        /// </remarks>
        void SetUnknown(T state);

        /// <summary>   Gets the type of the field. </summary>
        /// <value> The type. </value>
        Type Type { get; }

        /// <summary>
        /// Test to see if field is optional which means that an unknown value is legal.
        /// </summary>
        /// <returns>True if field is optional.</returns>
        bool Optional { get; }

        /// <summary>
        /// Test to see if field is nullable. 
        /// </summary>
        /// <returns>True if field is nullable.</returns>
        bool IsNullable { get; }

        /// <summary>
        /// Limits of numeric values.
        /// </summary>
        /// <param name="min">Minimum possible value.</param>
        /// <param name="max">Maximum possible value.</param>
        /// <returns>True if limits limit the underlying data type.</returns>
        /// <remarks>
        /// This typically reflects the result of setting <see cref="NumericAttribute"/> limits on the possible values.</remarks>
        bool Limits(out double min, out double max);

        /// <summary>
        /// Regular expression for validating a string.
        /// </summary>
        /// <returns>Validation regular expression.</returns>
        /// <remarks> This typically reflects the result of setting <see cref="PatternAttribute"/>.</remarks>
        string Pattern { get; }

        /// <summary>
        /// Returns the other fields this one depends on.
        /// </summary>
        /// <returns>List of field names this one depends on.</returns>
        /// <remarks>This is mainly useful for <see cref="Advanced.Confirmation{T}"/> fields.</remarks>
        IEnumerable<string> Dependencies { get; }
    }

    /// <summary>
    /// The role the field plays in a form.
    /// </summary>
    public enum FieldRole
    {
        /// <summary>
        /// Field is used to get a value to set in the form state.
        /// </summary>
        /// <remarks>This is the kind of field generated by <see cref="IFormBuilder{T}.Field"/>.</remarks>
        Value,

        /// <summary>
        /// Field is used to confirm some settings during the dialog.
        /// </summary>
        /// <remarks>
        /// This is the kind of field generated by <see cref="IFormBuilder{T}.Confirm"/>.
        /// </remarks>
        Confirm
    };

    /// <summary>
    /// Describe the information displayed about a field and its values.
    /// </summary>
    /// <remarks>
    /// Throughout this class Description refers to the name of a field or a value
    /// whereas "terms" tell what people can type to match the field or terms in it.
    /// When generating terms it is a good idea to include anything that might be reasonable
    /// for someone to type.  The form dialog itself will help clarify any ambiguity.  One
    /// way to do this is to use <see cref="TermsAttribute.MaxPhrase"/> which ensures that <see cref="Language.GenerateTerms"/>
    /// is called on your base terms.
    /// </remarks>
    public interface IFieldDescription
    {
        /// <summary>
        /// Role field plays in a form.
        /// </summary>
        /// <returns>Role field plays in form.</returns>
        FieldRole Role { get; }

        /// <summary>
        /// Description of the field itself.
        /// </summary>
        /// <returns>Field description.</returns>
        /// <remarks>
        /// This is the value that will be used in \ref patterns by {&amp;}, choices with {||} or buttons.
        /// </remarks>
        DescribeAttribute FieldDescription { get; }

        /// <summary>
        /// Terms for matching this field.
        /// </summary>
        /// <returns>List of term regex for matching the field name.</returns>
        IEnumerable<string> FieldTerms { get; }

        /// <summary>
        /// Return the description of a specific value.
        /// </summary>
        /// <param name="value">Value being described.</param>
        /// <returns>Description of value.</returns>
        DescribeAttribute ValueDescription(object value);

        /// <summary>
        /// Return all possible value descriptions in order to support enumeration.
        /// </summary>
        /// <returns>All possible value descriptions.</returns>
        IEnumerable<DescribeAttribute> ValueDescriptions { get; }

        /// <summary>
        /// Given a value return terms that can be used in a dialog to match the object.
        /// </summary>
        /// <param name="value">Value that would result from a match.</param>
        /// <returns>Enumeration of regex.</returns>
        IEnumerable<string> Terms(object value);

        /// <summary>
        /// All possible values or null if it is a data type like number.
        /// </summary>
        /// <returns>All possible values.</returns>
        IEnumerable<object> Values { get; }

        /// <summary>
        /// Are multiple matches allowed.
        /// </summary>
        /// <returns>True if more than one value is allowed.</returns>
        /// <remarks>This is true is you have a list of enumerated values.</remarks>
        bool AllowsMultiple { get; }

        /// <summary>
        /// Allow the default value as an option.
        /// </summary>
        /// <returns>True if default values are allowed.</returns>
        bool AllowDefault { get; }

        /// <summary>
        /// Allow user input to match numbers shown with enumerated choices. 
        /// </summary>
        /// <returns>True if numbers are allowed as input.</returns>
        bool AllowNumbers { get; }
    }

    #region Documentation
    /// <summary>   Interface for saving/localizing generated resources. </summary>
    #endregion
    public interface IFieldResources
    {
        /// <summary>   Adds any string resources to form localizer. </summary>
        void SaveResources();

        /// <summary>   Loads any string resources from the form localizer. </summary>
        void Localize();
    }

    /// <summary>
    /// Direction for next step.
    /// </summary>
    /// <remarks>
    /// As each step in a form completes, the step can determine the next step to take.
    /// Usually this is just to move onto the next active, uncompleted step, but you can 
    /// also move back or present a list of choices to the user.
    /// A step is active if <see cref="IFieldPrompt{T}.Active(T)"/> returns true on the current state.
    /// A step is ready if it has not already been successfully completed.
    /// </remarks>
    public enum StepDirection
    {
        /// <summary>
        /// The form is complete and <see cref="IFormBuilder{T}.OnCompletion(OnCompletionAsyncDelegate{T})"/> should be called.
        /// </summary>
        Complete,

        /// <summary>
        /// Move to a named step.  If there is more than one name, the user will be asked to choose.
        /// </summary>
        Named,

        /// <summary>
        /// Move to the next step that is <see cref="IFieldPrompt{T}.Active(T)"/> and uncompleted.
        /// </summary>
        Next,

        /// <summary>
        /// Move to the previously executed step.
        /// </summary>
        Previous,

        /// <summary>
        /// Quit the form and return failure to the parent dialog.
        /// </summary>
        Quit,

        /// <summary>
        /// Reset the form to start over.
        /// </summary>
        Reset
    };

    /// <summary>
    /// Next step to take.
    /// </summary>
    [Serializable]
    public class NextStep
    {
        /// <summary>
        /// By default move on to the next active, uncompleted step.
        /// </summary>
        public NextStep()
        {
            Direction = StepDirection.Next;
        }

        /// <summary>
        /// Move as specified in direction.
        /// </summary>
        /// <param name="direction">What step to do next.</param>
        public NextStep(StepDirection direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// Ask the user which of the fields to move to next.
        /// </summary>
        /// <param name="names">Enumeration of possible next steps.</param>
        public NextStep(IEnumerable<string> names)
        {
            Direction = StepDirection.Named;
            Names = names.ToArray();
        }

        /// <summary>
        /// Direction for next step.
        /// </summary>
        public StepDirection Direction;

        /// <summary>
        /// If this is a named step, one or more named steps to move to.  If there are more than one, the user will choose.
        /// </summary>
        public string[] Names;
    }

    /// <summary>
    /// This provides control information about a field.
    /// </summary>
    /// <typeparam name="T">Form state that is being completed.</typeparam>
    public interface IFieldPrompt<T>
       where T : class
    {
        /// <summary>
        /// Test to see if field is currently active based on the current state.
        /// </summary>
        /// <returns>True if field is active.</returns>
        /// <remarks>
        /// One way to control this is to supply a <see cref="ActiveDelegate{T}"/> to the 
        /// <see cref="IFormBuilder{T}.Field"/> or <see cref="IFormBuilder{T}.Confirm"/> steps.
        /// </remarks>
        bool Active(T state);

        /// <summary>
        /// Return a template for building a prompt.
        /// </summary>
        /// <param name="usage">Kind of template we are looking for.</param>
        /// <returns>NULL if no template, otherwise a template annotation.</returns>
        TemplateAttribute Template(TemplateUsage usage);

        #region Documentation
        /// <summary>   Returns the prompt description. </summary>
        /// <returns>   An <see cref="IPrompt{T}"/> describing prompt and recognizer. </returns>
        /// <remarks>If a prompt is dynamically computed this should be null until <see cref="DefineAsync(T)"/> is called.</remarks>
        #endregion
        IPrompt<T> Prompt { get; }

        /// <summary>
        /// Build the prompt and recognizer for dynamically defined fields.
        /// </summary>
        /// <returns>True if field is defined.</returns>
        /// <remarks>
        ///          This method is called before asking for <see cref="Prompt"/>.
        ///          This provides an opportunity to dynamically define the field based on the current
        ///          state or external information.  The <see cref="IFieldState{T}.Dependencies"/> method 
        ///          identifies fields that this one depends on.  All of them will be complete before the field
        ///          will be shown to the user, but this method might be called earlier in order to define the field
        ///          for things like status and initial matching or validation.
        /// </remarks>
        Task<bool> DefineAsync(T state);

        /// <summary>
        /// Validate value to be set on state and return feedback if not valid.
        /// </summary>
        /// <param name="state">State before setting value.</param>
        /// <param name="value">Value to be set in field.</param>
        /// <returns>Result including feedback and if valid.</returns>
        /// <remarks>
        /// One way to control this is to supply a <see cref="ValidateAsyncDelegate{T}"/> to the 
        /// <see cref="IFormBuilder{T}.Field"/> or <see cref="IFormBuilder{T}.Confirm"/> steps.
        /// </remarks>
        Task<ValidateResult> ValidateAsync(T state, object value);

        /// <summary>
        /// Return the help description for this field.
        /// </summary>
        /// <returns>The prompt to use for generating help.</returns>
        /// <remarks>
        /// Help is a mixture of field specific help, what a recognizer understands and available commands.
        /// </remarks>
        IPrompt<T> Help { get; }

        /// <summary>
        /// Next step to execute.
        /// </summary>
        /// <param name="value">Value in response to prompt.</param>
        /// <param name="state">Current form state.</param>
        /// <returns>Next step to execute.</returns>
        NextStep Next(object value, T state);
    }

    /// <summary>
    /// Interface for all the information about a specific field.
    /// </summary>
    /// <typeparam name="T">Form state interface applies to.</typeparam>
    public interface IField<T> : IFieldState<T>, IFieldDescription, IFieldPrompt<T>, IFieldResources
        where T : class
    {
        /// <summary>
        /// Name of this field.
        /// </summary>
        /// <returns>Name of this field.</returns>
        /// <remarks>
        /// For a value field this is the path in the form state that leads to the value being filled in.
        /// For a confirm field this is a randomly generated name.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Form that owns this field
        /// </summary>
        IForm<T> Form { get; set; }
    }

    /// <summary>
    /// Interface to track all of the fields in a form.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFields<T> : IEnumerable<IField<T>>
        where T : class
    {
        /// <summary>
        /// Return a specific field or null if not present.
        /// </summary>
        /// <param name="name">Name of field to find.</param>
        /// <returns>Field description for name or null.</returns>
        IField<T> Field(string name);
    }
}
