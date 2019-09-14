using Microsoft.Bot.Builder.Dialogs.Adaptive;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class FormEvents : AdaptiveEvents
    {
        public const string Ask = "ask";
        public const string ChooseSlot = "chooseSlot";
        public const string ChooseSlotValue = "chooseSlotValue";
        public const string ClarifySlotValue = "clarifySlotValue";
        public const string ClearSlot = "clearSlot";
        public const string FillForm = "fillForm";
        public const string NextFormEvent = "nextFormEvent";
        public const string SetSlot = "setSlot";
        public const string UnknownEntity = "unknownEntity";
    }
}
