using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    interface IComposableDialog 
    {
        /// <summary>
        ///  Interuption Dialog
        /// </summary>
        /// <remarks>configurable dialog which allows global intents to be handled</remarks>
        string InterruptionDialogId { get; set; }

        ///<summary>
        /// Fallback Dialog
        /// </summary>
        /// <remarks>Configured dialog which may be called by the dialog </remarks>
        string FallbackDialogId { get; set; }
    }
}
