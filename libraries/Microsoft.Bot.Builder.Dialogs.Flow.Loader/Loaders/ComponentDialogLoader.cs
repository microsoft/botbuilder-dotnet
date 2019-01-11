using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    /// <summary>
    /// Type loader specifically for ComponentDialog since
    /// needs custom loading of the dialogs collection.
    /// </summary>
    public class ComponentDialogLoader : DefaultLoader
    {
        public override object Load(JObject obj, JsonSerializer serializer, Type type)
        {
            ComponentDialog dialog = base.Load(obj, serializer, type) as ComponentDialog;

            // Dialogs are not a public property of ComponentDialog so we read the dialog
            // collection from the json and call AddDialog() on each dialog.
            if (dialog != null)
            {
                var dialogs = obj["Dialogs"];

                // If there are dialogs, load them.
                if (dialogs != null)
                {
                    if (obj["Dialogs"].Type != JTokenType.Array)
                    {
                        throw new JsonSerializationException("Expected array property \"Dialogs\" in ComponentDialog");
                    }

                    foreach (var dialogJObj in dialogs)
                    {
                        var innerDialog = dialogJObj.ToObject<IDialog>(serializer);
                        dialog.AddDialog(innerDialog);
                    }
                }
            }

            return dialog;
        }
    }
}
