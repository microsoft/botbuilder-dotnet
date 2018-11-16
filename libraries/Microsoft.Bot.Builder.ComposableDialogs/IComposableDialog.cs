using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    interface IComposableDialog : IDialog
    {
        /// <summary>
        /// Recognizer
        /// </summary>
        IRecognizer Recognizer { get; set; }

        /// <summary>
        ///  Interuption Dialog
        /// </summary>
        /// <remarks>configurable dialog which allows global intents to be handled</remarks>
        IDialog InterruptionDialog { get; set; }

        ///<summary>
        /// Fallback Dialog
        /// </summary>
        /// <remarks>Configured dialog which may be called by the dialog </remarks>
        IDialog FallbackDialog { get; set; }

        /// <summary>
        /// Slots
        /// </summary>
        /// <remarks>Slot definitions which are used </remarks>
        ISlot[] Slots { get; set; }
    }
}
