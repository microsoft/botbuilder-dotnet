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
        public void TestMultiLanguageLG()
        {
            var localPerFile = new Dictionary<string, string>
            {
                { "en", Path.Combine(AppContext.BaseDirectory, "MultiLanguage", "c.en.lg") },
                { string.Empty, Path.Combine(AppContext.BaseDirectory, "MultiLanguage", "c.lg") } // default local
            };

            var generator = new MultiLanguageLG(localPerFile);

            // fallback to "c.en.lg"
            var result = generator.Generate("${templatec()}", null, "en-us");
            Assert.AreEqual("from c.en.lg", result);

            // "c.en.lg" is used
            result = generator.Generate("${templatec()}", null, "en");
            Assert.AreEqual("from c.en.lg", result);

            // locale "fr" has no entry file, default file "c.lg" is used
            result = generator.Generate("${templatec()}", null, "fr");
            Assert.AreEqual("from c.lg", result);

            // "c.lg" is used
            result = generator.Generate("${templatec()}", null, null);
            Assert.AreEqual("from c.lg", result);
        }
    }
}
