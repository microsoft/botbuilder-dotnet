// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Contains settings to pass to a <see cref="Prompt{T}"/> when the prompt is started.
    /// </summary>
    public class PromptOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromptOptions"/> class.
        /// </summary>
        /// <param name="prompt">The initial prompt to send the user as an <see cref="Activity"/>.</param>
        /// <param name="choices">The list of available choices.</param>
        public PromptOptions(Activity prompt = default, IList<Choice> choices = default)
        {
            Prompt = prompt;
            Choices = choices;
        }

        /// <summary>
        /// Gets or sets the initial prompt to send the user as an <see cref="Activity"/>.
        /// </summary>
        /// <value>
        /// The initial prompt to send the user as and <see cref="Activity"/>.
        /// </value>
        public Activity Prompt { get; set; }

        /// <summary>
        /// Gets or sets the retry prompt to send the user as and <see cref="Activity"/>.
        /// </summary>
        /// <value>
        /// The retry prompt to send the user as an <see cref="Activity"/>.
        /// </value>
        public Activity RetryPrompt { get; set; }

        /// <summary>
        /// Gets a list of choices for the user to choose from, for use with a <see cref="ChoicePrompt"/>.
        /// </summary>
        /// <value>The list of available choices.</value>
        public IList<Choice> Choices { get; private set; } = new List<Choice>();

        /// <summary>
        /// Gets or sets the <see cref="ListStyle"/> for a <see cref="ChoicePrompt"/>.
        /// </summary>
        /// <value>The list style to use when presenting a choice prompt.</value>
        /// <remarks>
        /// This property can be used to override or set the value of <see cref="ChoicePrompt.Style"/> property
        /// when the prompt is started using <see cref="DialogContext.PromptAsync"/>.
        /// </remarks>
        public ListStyle? Style { get; set; }

        /// <summary>
        /// Gets or sets additional options for use with any <see cref="PromptValidator{T}"/> attached to the prompt.
        /// </summary>
        /// <value>Additional options for use with a prompt validator.</value>
        public object Validations { get; set; }
    }
}
