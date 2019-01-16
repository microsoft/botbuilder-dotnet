// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters
{
    public class DialogCommandConverter : InterfaceConverter<IDialogCommand>
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);

            if (jToken is JArray)
            {
                // If we have an array of IDialogCommands, create a CommandSet
                // and add all the array elements to make for a more compact format.
                // Users can still specify a CommandSet explicitly if they prefer so.
                var commandSet = new CommandSet();

                foreach (var comamndObj in jToken as JArray)
                {
                    var command = comamndObj.ToObject<IDialogCommand>(serializer);
                    commandSet.Add(command);
                }

                return commandSet;
            }

            // If we have a single command, call the generic InterfaceConverter<IDialogCommand>.
            // Note that since we did JToken.Load(reader) above, we already 'consumed' the json
            // in the deserialization process, so we want to create a new reader or otherwise
            // the base class will try to read from the JsonReader and it will be empty.
            return base.ReadJson(jToken.CreateReader(), objectType, existingValue, serializer);
        }

    }
}
