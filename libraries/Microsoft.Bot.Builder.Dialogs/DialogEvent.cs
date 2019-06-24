using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogEvent
    {
        /// <summary>
        /// If `true` the event will be bubbled to the parent `DialogContext` if not handled by the current dialog.
        /// </summary>
        public bool Bubble { get; set; }

        /// <summary>
        /// Name of the event being raised.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional value associated with the event.
        /// </summary>
        public object Value { get; set; }
    }
}
