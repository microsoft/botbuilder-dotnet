using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    interface IDispatchDialog 
    {
        /// <summary>
        /// Recognizer
        /// </summary>
        IRecognizer Recognizer { get; set; }

        /// <summary>
        /// Intent -> DialogId mappings
        /// </summary>
        Dictionary<string, string> Routes { get; set; }
    }
}
