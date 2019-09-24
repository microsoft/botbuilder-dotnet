namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public partial class FormDialog
    {
        // Slot and operation
        public class SlotOp
        {
            public string Slot { get; set; }

            public string Operation { get; set; }

            public override string ToString()
                => $"{Operation}({Slot})";
        }
    }
}
