using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public enum DialogConsultationDesires
    {
        /// <summary>
        /// The dialog can process the utterance but if parent dialogs should process it they can.
        /// </summary>
        CanProcess,

        /// <summary>
        /// The dialog should process the utterance.
        /// </summary>
        ShouldPrcess
    }

    public class DialogConsultation
    {
        /// <summary>
        /// Expresses the desire of the dialog to process the current utterance.
        /// </summary>
        public DialogConsultationDesires Desire { get; set; }

        /// <summary>
        /// Function that should be invoked to process the utterance.
        /// </summary>
        public Func<DialogContext, Task<DialogTurnResult>> Processor { get; set; }

    }
}
