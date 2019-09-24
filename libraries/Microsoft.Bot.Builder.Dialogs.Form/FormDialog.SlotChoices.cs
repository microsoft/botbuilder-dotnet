using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public partial class FormDialog
    {
        // Select which slot entity belongs to
        public class SlotChoices
        {
            public List<SlotOp> Slots { get; set; } = new List<SlotOp>();

            public EntityInfo Entity { get; set; }

            public override string ToString()
                => $"Slot {Entity} = [{Slots}]";
        }
    }
}
