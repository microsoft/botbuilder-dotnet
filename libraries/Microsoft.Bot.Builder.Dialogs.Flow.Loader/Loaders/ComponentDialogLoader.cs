using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Contract;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    /// <summary>
    /// Type loader specifically for ComponentDialog since
    /// needs custom loading of the dialogs collection.
    /// </summary>
    public class ComponentDialogLoader : ILoader
    {
        public object Load(JObject obj, JsonSerializer serializer, Type type)
        {
            if (obj["InitialDialogId"].Type != JTokenType.String)
            {
                throw new JsonSerializationException("Expected string property \"InitialDialogId\" in ComponentDialog");
            }

            if (obj["Dialogs"].Type != JTokenType.Array)
            {
                throw new JsonSerializationException("Expected array property \"Dialogs\" in ComponentDialog");
            }

            var dialogId = obj["Id"].Value<string>();
            var initialDialogId = obj["InitialDialogId"].Value<string>();
            var dialogs = obj["Dialogs"];

            var componentDialog = new ComponentDialog()
            {
                Id = dialogId,
                InitialDialogId = initialDialogId
            };

            foreach (var dialogJObj in dialogs)
            {
                var innerDialog = dialogJObj.ToObject<IDialog>(serializer);
                componentDialog.AddDialog(innerDialog);
            }

            return componentDialog;
        }
    }
}
