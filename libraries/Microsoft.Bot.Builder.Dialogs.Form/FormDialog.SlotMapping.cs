namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public partial class FormDialog
    {
        // Simple mapping
        public class SlotMapping
        {
            public SlotOp Change { get; set; }

            public EntityInfo Entity { get; set; }

            public override string ToString()
                => $"{Change} = {Entity}";
        }
    }
}
