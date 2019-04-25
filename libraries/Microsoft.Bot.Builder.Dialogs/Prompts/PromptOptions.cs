// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptOptions
    {
        /// <summary>
        /// Gets or sets the initial prompt to send the user as <seealso cref="Activity"/>.
        /// </summary>
        /// <value>
        /// The initial prompt to send the user as <seealso cref="Activity"/>.
        /// </value>
        public Activity Prompt { get; set; }

        /// <summary>
        /// Gets or sets the retry prompt to send the user as <seealso cref="Activity"/>.
        /// </summary>
        /// <value>
        /// The retry prompt to send the user as <seealso cref="Activity"/>.
        /// </value>
        public Activity RetryPrompt { get; set; }

        public IList<Choice> Choices { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ListStyle"/> for a <see cref="ChoicePrompt"/>.
        /// </summary>
        /// <value>
        /// This property can be used to override or set the value of <see cref="ChoicePrompt.Style"/> property
        /// when the prompt is being executed using <see cref="DialogContext.PromptAsync"/>.
        /// </value>
        public ListStyle? Style { get; set; }

        public object Validations { get; set; }
    }
}
