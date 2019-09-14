using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Form.Converters;
using Microsoft.Bot.Builder.Dialogs.Form.Events;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class FormComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            yield return new TypeRegistration<FormDialog>("Microsoft.FormDialog");
            yield return new TypeRegistration<OnAsk>("Microsoft.OnAsk");
            yield return new TypeRegistration<OnChooseSlot>("Microsoft.OnChooseSlot");
            yield return new TypeRegistration<OnChooseSlotValue>("Microsoft.OnChooseSlotValue");
            yield return new TypeRegistration<OnClarifySlotValue>("Microsoft.OnClarifySlotValue");
            yield return new TypeRegistration<OnClearSlot>("Microsoft.OnClearSlot");
            yield return new TypeRegistration<OnNextFormEvent>("Microsoft.OnNextFormEvent");
            yield return new TypeRegistration<OnSetSlot>("Microsoft.OnSetSlot");
        }

        public override IEnumerable<JsonConverter> GetConverters(Source.IRegistry registry, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new DialogSchemaConverter(refResolver, registry);
        }
    }
}
