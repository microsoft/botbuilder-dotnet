using System.IO;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form.Test
{
    [TestClass]
    public class FormTests
    {
        public static readonly string SchemaFile = @"resources\schema.json";

        [TestMethod]
        public void TestSchemaGeneration()
        {
            var obj = (JObject)new JsonSerializer().Deserialize(new JsonTextReader(new StreamReader(SchemaFile)));
            var schema = new DialogSchema(obj);
            var dialog = new FormDialog(null, schema);
            dialog.Recognizer = new LuisRecognizer("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/ec5be598-b4c5-4adb-9272-9bfb52595dec?verbose=true&timezoneOffset=-360&subscription-key=0f43266ab91447ec8d705897381478c5&q=",
                new LuisPredictionOptions
                {
                    IncludeInstanceData = true,
                });

        }
    }
}
