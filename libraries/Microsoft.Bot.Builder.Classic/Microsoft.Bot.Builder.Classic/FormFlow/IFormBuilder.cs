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
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.FormFlow
{
    #region Documentation
    /// <summary>   Given <paramref name="state"/> return a <see cref="PromptAttribute"/> with a template for the message to display. </summary>
    /// <typeparam name="T">    Form state type. </typeparam>
    /// <param name="state">    Form state. </param>
    /// <returns>   A PromptAttribute describing the message to display. </returns>
    #endregion
    public delegate Task<PromptAttribute> MessageDelegate<T>(T state);

    #region Documentation
    /// <summary>   Interface for building a form. </summary>
    /// <remarks>   
    /// A form consists of a series of steps that can be one of:
    /// <list type="list">
    /// <item>A message to the user.</item>
    /// <item>A prompt sent to the user where the response is to fill in a form state value.</item>
    /// <item>A confirmation of the current state with the user.</item>
    /// </list>
    /// By default the steps are executed in the order of the <see cref="Message"/>, <see cref="Field"/> and <see cref="Confirm"/> calls.
    /// If you do not take explicit control, the steps will be executed in the order defined in the 
    /// form state with a final confirmation.
    /// This interface allows you to flently build a form by composing together fields,
    /// messages and confirmation.  The fluent building blocks provide common patterns
    /// like fields being based on your state class, but you can also build up your
    /// own definition of a form by using Advanced.IField.  
    /// If you want to build a form using C# reflection over your state class use FormBuilder.  
    /// To declaratively build a form through JSON Schema you can use Json.FormBuilderJson.
    /// 
    /// Forms are sensitive to the current thread UI culture.  The Microsoft.Bot.Builder.Classic strings will localize
    /// to that culture if available.  You can also localize the strings generated for your form by calling IForm.SaveResources
    /// or by using the RView tool and adding that resource to your project.  For strings in dynamic fields, messages or confirmations you will
    /// need to use the normal C# mechanisms to localize them.  Look in the overview documentation for more information.
    /// </remarks>
    /// <typeparam name="T">Form state.</typeparam>
    #endregion
    public interface IFormBuilder<T>
        where T : class
    {
        /// <summary>
        /// Build the form based on the methods called on the builder.
        /// </summary>
        /// <param name="resourceAssembly">Assembly for localization resources.</param>
        /// <param name="resourceName">Name of resources to use for localization.</param>
        /// <returns>The constructed form.</returns>
        /// <remarks>
        /// The default assembly is the one that contains <typeparamref name="T"/>
        /// and the default resourceName if the name of that type.
        /// </remarks>
        IForm<T> Build(Assembly resourceAssembly = null, string resourceName = null);

        /// <summary>
        /// The form configuration supplies default templates and settings.
        /// </summary>
        /// <returns>The current form configuration.</returns>
        FormConfiguration Configuration { get; }

        /// <summary>
        /// Show a message that does not require a response.
        /// </summary>
        /// <param name="message">A \ref patterns string to fill in and send.</param>
        /// <param name="condition">Whether or not this step is active.</param>
        /// <param name="dependencies">Fields message depends on.</param>
        /// <returns>Modified IFormBuilder.</returns>
        IFormBuilder<T> Message(string message, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null);

        /// <summary>
        /// Show a message with more format control that does not require a response.
        /// </summary>
        /// <param name="prompt">Message to fill in and send.</param>
        /// <param name="condition">Whether or not this step is active.</param>
        /// <param name="dependencies">Fields message depends on.</param>
        /// <returns>Modified IFormBuilder.</returns>
        IFormBuilder<T> Message(PromptAttribute prompt, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null);

        #region Documentation
        /// <summary>   Generate a message using a delegate to dynamically build the message. </summary>
        /// <param name="generateMessage">  Delegate for building message. </param>
        /// <param name="condition">        Whether or not this step is active. </param>
        /// <param name="dependencies">Fields message depends on.</param>
        /// <returns>Modified IFormBuilder.</returns>
        #endregion
        IFormBuilder<T> Message(MessageDelegate<T> generateMessage, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null);

        /// <summary>
        /// Derfine a field step by supplying your own field definition.
        /// </summary>
        /// <param name="field">Field definition to use.</param>
        /// <returns>Modified IFormBuilder.</returns>
        /// <remarks>
        /// You can provide your own implementation of <see cref="IField{T}"/> or you can 
        /// use the <see cref="Field{T}"/> class to provide fluent values, 
        /// <see cref="FieldReflector{T}"/> to use reflection or Json.FieldJson to use JSON Schema.
        /// </remarks>
        IFormBuilder<T> Field(IField<T> field);

        /// <summary>
        /// Define a step for filling in a particular value in the form state.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="active">Delegate to test form state to see if step is active.</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>Modified IFormBuilder.</returns>
        IFormBuilder<T> Field(string name, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null);

        /// <summary>
        /// Define a step for filling in a particular value in the form state.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="prompt">Simple \ref patterns to describe prompt for field.</param>
        /// <param name="active">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>Modified IFormBuilder.</returns>
        IFormBuilder<T> Field(string name, string prompt, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null);

        /// <summary>
        /// Define a step for filling in a particular value in the form state.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="prompt">Prompt pattern with more formatting control to describe prompt for field.</param>
        /// <param name="active">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>Modified IFormBuilder.</returns>
        IFormBuilder<T> Field(string name, PromptAttribute prompt, ActiveDelegate<T> active = null, ValidateAsyncDelegate<T> validate = null);

        /// <summary>
        /// Add all fields not already added to the form.
        /// </summary>
        /// <param name="exclude">Fields not to include.</param>
        /// <returns>Modified IFormBuilder.</returns>
        /// <remarks>
        /// This will add all fields defined in your form that have not already been
        /// added if the fields are supported.
        /// </remarks>
        IFormBuilder<T> AddRemainingFields(IEnumerable<string> exclude = null);

        /// <summary>
        /// Add a confirmation step.
        /// </summary>
        /// <param name="prompt">Prompt to use for confirmation.</param>
        /// <param name="condition">Delegate to test if confirmation applies to the current form state.</param>
        /// <param name="dependencies">What fields this confirmation depends on.</param>
        /// <returns>Modified IFormBuilder.</returns>
        /// <remarks>
        /// If prompt is not supplied the \ref patterns element {*} will be used to confirm.
        /// Dependencies will by default be all active steps defined before this confirmation.
        /// </remarks>
        IFormBuilder<T> Confirm(string prompt = null, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null);

        /// <summary>
        /// Add a confirmation step.
        /// </summary>
        /// <param name="prompt">Prompt to use for confirmation.</param>
        /// <param name="condition">Delegate to test if confirmation applies to the current form state.</param>
        /// <param name="dependencies">What fields this confirmation depends on.</param>
        /// <returns>Modified IFormBuilder.</returns>
        /// <remarks>
        /// Dependencies will by default be all active steps defined before this confirmation.
        /// </remarks>
        IFormBuilder<T> Confirm(PromptAttribute prompt, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null);

        #region Documentation
        /// <summary>   Generate a confirmation using a delegate to dynamically build the message. </summary>
        /// <param name="generateMessage">  Delegate for building message. </param>
        /// <param name="condition">        Whether or not this step is active. </param>
        /// <param name="dependencies">What fields this confirmation depends on.</param>
        /// <returns>Modified IFormBuilder.</returns>
        #endregion
        IFormBuilder<T> Confirm(MessageDelegate<T> generateMessage, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null);


        /// <summary>
        /// Delegate to send prompt to user.
        /// </summary>
        /// <param name="prompter">Delegate.</param>
        /// <returns>Modified IFormBuilder.</returns>
        IFormBuilder<T> Prompter(PromptAsyncDelegate<T> prompter);

        /// <summary>
        /// Delegate to call when form is completed.
        /// </summary>
        /// <param name="callback">Delegate to call on completion.</param>
        /// <returns>Modified IFormBuilder.</returns>
        /// <remarks>
        /// This should only be used for side effects such as calling your service with
        /// the form state results.  In any case the completed form state will be passed
        /// to the parent dialog.
        /// </remarks>
        IFormBuilder<T> OnCompletion(OnCompletionAsyncDelegate<T> callback);

        /// <summary>
        /// Test to see if there is already a field with <paramref name="name"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>True if field is already present.</returns>
        bool HasField(string name);
    }

    /// <summary>
    /// Default values for the form.
    /// </summary>
    /// <remarks>
    /// These defaults can all be overridden when you create a form and before you add steps.
    /// </remarks>
    public class FormConfiguration
    {
        /// <summary>
        /// Construct configuration.
        /// </summary>
        public FormConfiguration()
        {
            DefaultPrompt.IsLocalizable = false;
            foreach (var template in Templates)
            {
                template.IsLocalizable = false;
            }
        }

        /// <summary>
        /// Default prompt and template format settings.
        /// </summary>
        /// <remarks>
        /// When you specify a <see cref="PromptAttribute"/> or <see cref="TemplateAttribute"/>, any format 
        /// value you do not specify will come from this default.
        /// </remarks>
        public PromptAttribute DefaultPrompt = new PromptAttribute(string.Empty)
        {
            AllowDefault = BoolDefault.True,
            ChoiceCase = CaseNormalization.None,
            ChoiceFormat = Resources.DefaultChoiceFormat,
            ChoiceLastSeparator = Resources.DefaultChoiceLastSeparator,
            ChoiceParens = BoolDefault.True,
            ChoiceSeparator = Resources.DefaultChoiceSeparator,
            ChoiceStyle = ChoiceStyleOptions.Auto,
            FieldCase = CaseNormalization.Lower,
            Feedback = FeedbackOptions.Auto,
            LastSeparator = Resources.DefaultLastSeparator,
            Separator = Resources.DefaultSeparator,
            ValueCase = CaseNormalization.InitialUpper
        };

        /// <summary>
        /// Enumeration of strings for interpreting a user response as setting an optional field to be unspecified.
        /// </summary>
        /// <remarks>
        /// The first string is also used to describe not having a preference for an optional field.
        /// </remarks>
        public string[] NoPreference = Resources.MatchNoPreference.SplitList();

        /// <summary>
        /// Enumeration of strings for interpreting a user response as asking for the current value.
        /// </summary>
        /// <remarks>
        /// The first value is also used to describe the option of keeping the current value.
        /// </remarks>
        public string[] CurrentChoice = Resources.MatchCurrentChoice.SplitList();

        /// <summary>
        /// Enumeration of values for a "yes" response for boolean fields or confirmations.
        /// </summary>
        public string[] Yes = Resources.MatchYes.SplitList();

        /// <summary>
        /// Enumeration of values for a "no" response for boolean fields or confirmations.
        /// </summary>
        public string[] No = Resources.MatchNo.SplitList();

        /// <summary>
        /// string for naming the "navigation" field.
        /// </summary>
        public string Navigation = Resources.Navigation;

        /// <summary>
        /// string for naming "Confirmation" fields.
        /// </summary>
        public string Confirmation = Resources.Confirmation;

        /// <summary>
        /// Default templates to use if not override on the class or field level.
        /// </summary>
        public List<TemplateAttribute> Templates = new List<TemplateAttribute>
        {
            new TemplateAttribute(TemplateUsage.Bool, Resources.TemplateBool),
            // {0} is current choice, {1} is no preference
            new TemplateAttribute(TemplateUsage.BoolHelp, Resources.TemplateBoolHelp),

            // {0} is term being clarified
            new TemplateAttribute(TemplateUsage.Clarify, Resources.TemplateClarify),

            new TemplateAttribute(TemplateUsage.Confirmation, Resources.TemplateConfirmation),

            new TemplateAttribute(TemplateUsage.CurrentChoice, Resources.TemplateCurrentChoice),

            new TemplateAttribute(TemplateUsage.DateTime, Resources.TemplateDateTime),
            // {0} is current choice, {1} is no preference
            // new TemplateAttribute(TemplateUsage.DateTimeHelp, "Please enter a date or time expression like 'Monday' or 'July 3rd'{?, {0}}{?, {1}}."),
            new TemplateAttribute(TemplateUsage.DateTimeHelp, Resources.TemplateDateTimeHelp),

            // {0} is min and {1} is max.
            new TemplateAttribute(TemplateUsage.Double, Resources.TemplateDouble) { ChoiceFormat = Resources.TemplateDoubleChoiceFormat },
            // {0} is current choice, {1} is no preference
            // {2} is min and {3} is max
            new TemplateAttribute(TemplateUsage.DoubleHelp, Resources.TemplateDoubleHelp),

            // {0} is min, {1} is max and {2} are enumerated descriptions
            new TemplateAttribute(TemplateUsage.EnumManyNumberHelp, Resources.TemplateEnumManyNumberHelp),
            new TemplateAttribute(TemplateUsage.EnumOneNumberHelp, Resources.TemplateEnumOneNumberHelp),

            // {2} are the words people can type
            new TemplateAttribute(TemplateUsage.EnumManyWordHelp, Resources.TemplateEnumManyWordHelp),
            new TemplateAttribute(TemplateUsage.EnumOneWordHelp, Resources.TemplateEnumOneWordHelp),

            new TemplateAttribute(TemplateUsage.EnumSelectOne, Resources.TemplateEnumSelectOne),
            new TemplateAttribute(TemplateUsage.EnumSelectMany, Resources.TemplateEnumSelectMany),

            // {0} is the not understood term
            new TemplateAttribute(TemplateUsage.Feedback, Resources.TemplateFeedback),

            // For {0} is recognizer help and {1} is command help.
            new TemplateAttribute(TemplateUsage.Help, Resources.TemplateHelp),
            new TemplateAttribute(TemplateUsage.HelpClarify, Resources.TemplateHelpClarify),
            new TemplateAttribute(TemplateUsage.HelpConfirm, Resources.TemplateHelpConfirm),
            new TemplateAttribute(TemplateUsage.HelpNavigation, Resources.TemplateHelpNavigation),

            // {0} is min and {1} is max if present
            new TemplateAttribute(TemplateUsage.Integer, Resources.TemplateInteger) { ChoiceFormat = Resources.TemplateIntegerChoiceFormat },
            // {0} is current choice, {1} is no preference
            // {2} is min and {3} is max
            new TemplateAttribute(TemplateUsage.IntegerHelp, Resources.TemplateIntegerHelp),

            new TemplateAttribute(TemplateUsage.Navigation, Resources.TemplateNavigation) { FieldCase = CaseNormalization.None },
            // {0} is list of field names.
            new TemplateAttribute(TemplateUsage.NavigationCommandHelp, Resources.TemplateNavigationCommandHelp),
            new TemplateAttribute(TemplateUsage.NavigationFormat, Resources.TemplateNavigationFormat) {FieldCase = CaseNormalization.None },
            // {0} is min, {1} is max
            new TemplateAttribute(TemplateUsage.NavigationHelp, Resources.TemplateNavigationHelp),

            new TemplateAttribute(TemplateUsage.NoPreference, Resources.TemplateNoPreference),

            // {0} is the term that is not understood
            new TemplateAttribute(TemplateUsage.NotUnderstood, Resources.TemplateNotUnderstood),

            new TemplateAttribute(TemplateUsage.StatusFormat, Resources.TemplateStatusFormat) {FieldCase = CaseNormalization.None },

            new TemplateAttribute(TemplateUsage.String, Resources.TemplateString) { ChoiceFormat = Resources.TemplateStringChoiceFormat },
            // {0} is current choice, {1} is no preference
            new TemplateAttribute(TemplateUsage.StringHelp, Resources.TemplateStringHelp),

            new TemplateAttribute(TemplateUsage.Unspecified, Resources.TemplateUnspecified)
        };

        /// <summary>
        /// Definitions of the built-in commands.
        /// </summary>
        public Dictionary<FormCommand, CommandDescription> Commands = new Dictionary<FormCommand, CommandDescription>()
        {
            {FormCommand.Backup, new CommandDescription(
                Resources.CommandBack,
                Resources.CommandBackTerms.SplitList(),
                Resources.CommandBackHelp) },
            {FormCommand.Help, new CommandDescription(
                Resources.CommandHelp,
                Resources.CommandHelpTerms.SplitList(),
                Resources.CommandHelpHelp) },
            {FormCommand.Quit, new CommandDescription(
                Resources.CommandQuit,
                Resources.CommandQuitTerms.SplitList(),
                Resources.CommandQuitHelp) },
            {FormCommand.Reset, new CommandDescription(
                Resources.CommandReset,
                Resources.CommandResetTerms.SplitList(),
                Resources.CommandResetHelp)},
            {FormCommand.Status, new CommandDescription(
                Resources.CommandStatus,
                Resources.CommandStatusTerms.SplitList(),
                Resources.CommandStatusHelp) }
        };

        /// <summary>
        /// Look up a particular template.
        /// </summary>
        /// <param name="usage">Desired template.</param>
        /// <returns>Matching template.</returns>
        public TemplateAttribute Template(TemplateUsage usage)
        {
            TemplateAttribute result = null;
            foreach (var template in Templates)
            {
                if (template.Usage == usage)
                {
                    result = template;
                    break;
                }
            }
            Debug.Assert(result != null);
            return result;
        }
    };
}
