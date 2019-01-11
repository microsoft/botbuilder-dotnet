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
            var dialog = JsonConvert.DeserializeObject<IDialog>(
                json, new JsonSerializerSettings()
                {
                    SerializationBinder = new UriTypeBinder(),
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = new List<JsonConverter>()
                    {
                        new InterfaceConverter<IDialog>(),
                        new InterfaceConverter<IDialogCommand>(),
                        new InterfaceConverter<IRecognizer>(),
                        new ExpressionConverter(),
                        new ActivityConverter()
                    },
                    Error = (sender, args) =>
                    {
                        var ctx = args.ErrorContext;
                    }
                });
            return dialog;
        }
    }
}
