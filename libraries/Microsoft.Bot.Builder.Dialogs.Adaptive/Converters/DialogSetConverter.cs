using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    public class DialogSetConverter : JsonConverter<DialogSet>
    {
        private readonly ResourceExplorer resourceExplorer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogSetConverter"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resource explorer to use for resolving references.</param>
        public DialogSetConverter(ResourceExplorer resourceExplorer)
        {
            this.resourceExplorer = resourceExplorer;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Newtonsoft.Json.JsonConverter"/> can read JSON.
        /// </summary>
        /// <value>
        /// <c>true</c>.
        /// </value>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override DialogSet ReadJson(JsonReader reader, Type objectType, DialogSet existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            var result = new DialogSet();
            if (jToken is JArray arr)
            {
                foreach (var element in arr)
                {
                    string dialogName = element.ToObject<string>();
                    var resourceId = resourceExplorer.GetResource($"{dialogName}.dialog");
                    var dialog = resourceExplorer.LoadType<Dialog>(resourceId);
                    result.Add(dialog);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, DialogSet value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
