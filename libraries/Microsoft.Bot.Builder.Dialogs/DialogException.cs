using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Exception thrown in the context of a dialog with dialog context information.
    /// </summary>
    public class DialogException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogException"/> class.
        /// </summary>
        /// <param name="dialogContext">dialog Context.</param>
        public DialogException(DialogContext dialogContext)
        {
            SetPropertiesFromDc(dialogContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogException"/> class.
        /// </summary>
        /// <param name="dialogContext">dialog Context.</param>
        /// <param name="message">message text.</param>
        public DialogException(DialogContext dialogContext, string message)
            : base(message)
        {
            SetPropertiesFromDc(dialogContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogException"/> class.
        /// </summary>
        /// <param name="dialogContext">dialog Context.</param>
        /// <param name="message">message to include.</param>
        /// <param name="innerException">inner exception.</param>
        public DialogException(DialogContext dialogContext, string message, Exception innerException)
            : base(message, innerException)
        {
            SetPropertiesFromDc(dialogContext);
        }

        /// <summary>
        /// Gets or sets the active dialog data.
        /// </summary>
        /// <value>
        /// The dialog context active when the exception was thrown.
        /// </value>
        public DialogInstance ActiveDialog { get; set; }

        /// <summary>
        /// Gets or sets the active dialog data.
        /// </summary>
        /// <value>
        /// The dialog context active when the exception was thrown.
        /// </value>
        public DialogInstance Parent { get; set; }

        /// <summary>
        /// Gets or sets the active dialog data.
        /// </summary>
        /// <value>
        /// The dialog context active when the exception was thrown.
        /// </value>
        public List<DialogInstance> Stack { get; set; }

        private void SetPropertiesFromDc(DialogContext dialogContext)
        {
            this.ActiveDialog = dialogContext.ActiveDialog;
            this.Parent = dialogContext.Parent?.ActiveDialog;
            this.Stack = dialogContext.Stack;

            // put state snapshot into data.
            foreach (var memory in dialogContext.State.GetMemorySnapshot())
            {
                this.Data[memory.Key] = memory.Value;
            }
        }
    }
}
