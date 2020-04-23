// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class ActivityTemplateConverterTests
    {
        [TestMethod]
        [DataRow("{'activity': 'blah'}")]
        public void ReadsActivityTemplateJson(string jsonText)
        {
            // TODO: check with tom why this test case doesn't work:
            // [DataRow("{'activity': { '$kind': 'Microsoft.ActivityTemplate', 'template': 'blah'}")]
            var jsonReader = CreateJsonReader(jsonText);
            var sut = new ActivityTemplateConverter();
            var result = sut.ReadJson(jsonReader, null, null, null);
            Assert.IsInstanceOfType(result, typeof(ActivityTemplate));
            var template = (ActivityTemplate)result;
            Assert.AreEqual("blah", template.Template);
        }

        [TestMethod]
        public void ReadsStaticActivityTemplateActivityTemplateJson()
        {
            var jsonText = @"{'activity': {
            '$kind': 'Microsoft.StaticActivityTemplate',
            'activity': {
                'type': 'event',
                'name': 'BookFlight'
            }
            }}";
            var jsonReader = CreateJsonReader(jsonText);
            var sut = new ActivityTemplateConverter();
            var result = sut.ReadJson(jsonReader, null, null, null);
            Assert.IsInstanceOfType(result, typeof(StaticActivityTemplate));
            var template = (StaticActivityTemplate)result;
            Assert.AreEqual(ActivityTypes.Event, template.Activity.Type);
            Assert.AreEqual("BookFlight", template.Activity.Name);
        }

        private static JsonTextReader CreateJsonReader(string jsonText)
        {
            var jsonReader = new JsonTextReader(new StringReader(jsonText));

            // Advance the reader to the token we care about.
            jsonReader.Read();
            jsonReader.Read();
            jsonReader.Read();
            return jsonReader;
        }
    }
}
