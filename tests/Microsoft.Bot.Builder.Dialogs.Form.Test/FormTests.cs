using System.IO;
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

        }
    }
}
