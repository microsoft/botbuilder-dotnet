using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    /// <summary>
    /// A dialog which has a Recognizer
    /// </summary>
    public interface IRecognizerDialog<T> 
    {
        /// <summary>
        /// Recognizer
        /// </summary>
        IRecognizer Recognizer { get; set; }

        IDictionary<string, T> Routes { get; set; }
    }
}
