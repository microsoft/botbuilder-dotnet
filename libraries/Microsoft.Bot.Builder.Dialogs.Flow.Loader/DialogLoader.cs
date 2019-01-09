using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Contract;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader
{
    public static class DialogLoader
    {
        public static IDialog Load(string json)
        {
            var flowInfo = JsonConvert.DeserializeObject<DialogsInfo>(
                json, new JsonSerializerSettings()
                {
                    Binder = new Binder(),
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = new List<JsonConverter>()
                    {
                        new DialogCommandConverter(),
                        new DialogConverter(),
                        new ExpressionConverter(),
                        new ActivityConverter()
                    }
                });

            var rootDialog = new ComponentDialog()
            {
                Id = Guid.NewGuid().ToString(),
                InitialDialogId = flowInfo.InitialNodeId
            };

            foreach (var flowNodeInfo in flowInfo.Dialogs)
            {
                rootDialog.AddDialog(flowNodeInfo.Dialog);

                var flowDialog = new CommandDialog()
                {
                    Id = flowNodeInfo.Id,
                    DialogId = flowNodeInfo.Dialog.Id,
                    Command = new CommandSet()
                    {
                        Commands = flowNodeInfo.Commands
                    }
                };

                rootDialog.AddDialog(flowDialog);
            }

            return rootDialog;
        }
    }
}
