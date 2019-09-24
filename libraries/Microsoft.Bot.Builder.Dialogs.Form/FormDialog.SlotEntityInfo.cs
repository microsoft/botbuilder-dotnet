namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public partial class FormDialog
    {
        // Proposed mapping
        public class SlotEntityInfo
        {
            public PropertySchema Slot { get; set; }

            public EntityInfo Entity { get; set; }

            public bool Expected { get; set; }

            public override string ToString()
            {
                var expected = Expected ? "expected" : string.Empty;
                return $"{expected} {Slot} = {Entity.Name}";
            }
        }
    }
}
