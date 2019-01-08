using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    interface IDispatchDialog 
    {
        /// <summary>
        /// Recognizer
        /// </summary>
        IRecognizer Recognizer { get; set; }
    }
}
