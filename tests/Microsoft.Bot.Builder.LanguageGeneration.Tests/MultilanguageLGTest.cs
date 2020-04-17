using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.LanguageGeneration.Tests
{
    [TestClass]
    public class MultilanguageLGTest
    {
        [TestMethod]
        public void EmptyFallbackLocale()
        {
            var localPerFile = new Dictionary<string, string>
            {
                { "en", Path.Combine(AppContext.BaseDirectory, "MultiLanguage", "a.en.lg") },
                { string.Empty, Path.Combine(AppContext.BaseDirectory, "MultiLanguage", "a.lg") } // default local
            };

            var generator = new MultiLanguageLG(localPerFile);

            // fallback to "a.en.lg"
            var result = generator.Generate("templatec", null, "en-us");
            Assert.AreEqual("from a.en.lg", result);

            // "a.en.lg" is used
            result = generator.Generate("templatec", null, "en");
            Assert.AreEqual("from a.en.lg", result);

            // locale "fr" has no entry file, default file "a.lg" is used
            result = generator.Generate("templatec", null, "fr");
            Assert.AreEqual("from a.lg", result);

            // "a.lg" is used
            result = generator.Generate("templatec", null, null);
            Assert.AreEqual("from a.lg", result);
        }

        [TestMethod]
        public void SpecificFallbackLocale()
        {
            var localPerFile = new Dictionary<string, string>
            {
                { "en", Path.Combine(AppContext.BaseDirectory, "MultiLanguage", "a.en.lg") },
            };

            var generator = new MultiLanguageLG(localPerFile, "en");

            // fallback to "a.en.lg"
            var result = generator.Generate("templatec", null, "en-us");
            Assert.AreEqual("from a.en.lg", result);

            // "a.en.lg" is used
            result = generator.Generate("templatec", null, "en");
            Assert.AreEqual("from a.en.lg", result);

            // locale "fr" has no entry file, default file "a.en.lg" is used
            result = generator.Generate("templatec", null, "fr");
            Assert.AreEqual("from a.en.lg", result);

            // "a.en.lg" is used
            result = generator.Generate("templatec", null, null);
            Assert.AreEqual("from a.en.lg", result);
        }

        [TestMethod]
        public void TemplatesInputs()
        {
            var enTemplates = Templates.ParseText("[import](1.lg)\r\n # template\r\n - hi", "abc", ConstantResolver);
            var templatesDict = new Dictionary<string, Templates>
            {
                { "en", enTemplates },
            };

            var generator = new MultiLanguageLG(templatesDict, "en");

            // fallback to "a.en.lg"
            var result = generator.Generate("myTemplate", null, "en-us");
            Assert.AreEqual("content with id: 1.lg from source: abc", result);

            // "a.en.lg" is used
            result = generator.Generate("myTemplate", null, "en");
            Assert.AreEqual("content with id: 1.lg from source: abc", result);

            // locale "fr" has no entry file, default file "a.en.lg" is used
            result = generator.Generate("myTemplate", null, "fr");
            Assert.AreEqual("content with id: 1.lg from source: abc", result);

            // "a.en.lg" is used
            result = generator.Generate("myTemplate", null, null);
            Assert.AreEqual("content with id: 1.lg from source: abc", result);
        }

        private static (string content, string id) ConstantResolver(string sourceId, string resourceId)
        {
            return ($"# myTemplate\r\n - content with id: {resourceId} from source: {sourceId}", sourceId + resourceId);
        }
    }
}
