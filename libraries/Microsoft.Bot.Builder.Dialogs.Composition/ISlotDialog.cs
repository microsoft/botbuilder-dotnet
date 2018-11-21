using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    interface ISlotDialog 
    {
        /// <summary>
        /// Slots
        /// </summary>
        /// <remarks>Slot definitions which are used </remarks>
        List<ISlot> Slots { get; set; }
    }
}
