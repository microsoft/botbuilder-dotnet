using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public partial class FormDialog
    {
        // Select from multiple entities for singleton
        public class SingletonChoices
        {
            public List<EntityInfo> Entities { get; set; } = new List<EntityInfo>();

            public SlotOp Slot { get; set; }

            public override string ToString()
                => $"Singleton {Slot} = [{Entities}]";
        }
    }
}
