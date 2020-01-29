using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Templates.Tests
{
    [TestClass]
    public class SimpleMultiLangGeneratorTests
    {
        [TestMethod]
        public async Task TestInlineActivityFactory()
        {
            var localPerFile = new Dictionary<string, string>
            {
                { "en", Path.Combine(AppContext.BaseDirectory, "lg", "c.en.lg") },
                { string.Empty, Path.Combine(AppContext.BaseDirectory, "lg", "c.lg") } // default local
            };

            var generator = new SimpleMultiLangGenerator(localPerFile);

            // fallback to "c.en.lg"
            var result = await generator.Generate(GetTurnContext("en-us"), "@{templatec()}", null);
            Assert.AreEqual("from c.en.lg", result);

            // "c.en.lg" is used
            result = await generator.Generate(GetTurnContext("en"), "@{templatec()}", null);
            Assert.AreEqual("from c.en.lg", result);

            // locale "fr" has no entry file, default file "c.lg" is used
            result = await generator.Generate(GetTurnContext("fr"), "@{templatec()}", null);
            Assert.AreEqual("from c.lg", result);

            // "c.lg" is used
            result = await generator.Generate(GetTurnContext(null), "@{templatec()}", null);
            Assert.AreEqual("from c.lg", result);
        }

        private TurnContext GetTurnContext(string locale)
        {
            return new TurnContext(
                new TestAdapter(),
                new Activity() { Locale = locale ?? string.Empty, Text = string.Empty });
        }
    }
}
