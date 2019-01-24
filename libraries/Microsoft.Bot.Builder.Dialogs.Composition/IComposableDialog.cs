//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Bot.Builder.Dialogs;

//namespace Microsoft.Bot.Builder.Dialogs.Composition
//{
//    public interface IComposableDialog : IDialog
//    {
//        /// <summary>
//        ///  Interuption Dialog
//        /// </summary>
//        /// <remarks>configurable dialog which allows global intents to be handled</remarks>
//        IDialog InterruptionDialog { get; set; }

//        ///<summary>
//        /// Fallback Dialog
//        /// </summary>
//        /// <remarks>Configured dialog which may be called by the dialog </remarks>
//        IDialog FallbackDialog { get; set; }

//        /// <summary>
//        /// Dialog resources
//        /// </summary>
//        IDictionary<string, IDialog> Dialogs { get; set; }

//        /// <summary>
//        /// Slot definitions
//        /// </summary>
//        IDictionary<string, IValue> Slots { get; set; }
//    }

//}
