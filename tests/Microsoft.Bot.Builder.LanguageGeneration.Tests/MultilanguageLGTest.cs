using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Microsoft.Bot.Builder.LanguageGeneration.Tests
{
    public class MultilanguageLGTest
    {
        [Fact]
        public void EmptyFallbackLocale()
        {
            var localPerFile = new Dictionary<string, string>
            {
                { "en", Path.Combine(AppContext.BaseDirectory, "MultiLanguage", "a.en.lg") },
                { string.Empty, Path.Combine(AppContext.BaseDirectory, "MultiLanguage", "a.lg") } // default local
            };

            var generator = new MultiLanguageLG(localPerFile);

            // fallback to "a.en.lg"
            var result = generator.Generate("templatec", locale: "en-us");
            Assert.Equal("from a.en.lg", result);

            // "a.en.lg" is used
            result = generator.Generate("templatec", locale: "en");
            Assert.Equal("from a.en.lg", result);

            // locale "fr" has no entry file, default file "a.lg" is used
            result = generator.Generate("templatec", locale: "fr");
            Assert.Equal("from a.lg", result);

            // "a.lg" is used
            result = generator.Generate("templatec");
            Assert.Equal("from a.lg", result);
        }

        [Fact]
        public void SpecificFallbackLocale()
        {
            var localPerFile = new Dictionary<string, string>
            {
                { "en", Path.Combine(AppContext.BaseDirectory, "MultiLanguage", "a.en.lg") },
            };

            var generator = new MultiLanguageLG(localPerFile, "en");

            // fallback to "a.en.lg"
            var result = generator.Generate("templatec", locale: "en-us");
            Assert.Equal("from a.en.lg", result);

            // "a.en.lg" is used
            result = generator.Generate("templatec", locale: "en");
            Assert.Equal("from a.en.lg", result);

            // locale "fr" has no entry file, default file "a.en.lg" is used
            result = generator.Generate("templatec", locale: "fr");
            Assert.Equal("from a.en.lg", result);

            // "a.en.lg" is used
            result = generator.Generate("templatec");
            Assert.Equal("from a.en.lg", result);
        }

        [Fact]
        public void TemplatesInputs()
        {
            var enTemplates = Templates.ParseText("[import](1.lg)\r\n # template\r\n - hi", "abc", ConstantResolver);
            var templatesDict = new Dictionary<string, Templates>
            {
                { "en", enTemplates },
            };

            var generator = new MultiLanguageLG(templatesDict, "en");

            // fallback to "a.en.lg"
            var result = generator.Generate("myTemplate", locale: "en-us");
            Assert.Equal("content with id: 1.lg from source: abc", result);

            // "a.en.lg" is used
            result = generator.Generate("myTemplate", locale: "en");
            Assert.Equal("content with id: 1.lg from source: abc", result);

            // locale "fr" has no entry file, default file "a.en.lg" is used
            result = generator.Generate("myTemplate", locale: "fr");
            Assert.Equal("content with id: 1.lg from source: abc", result);

            // "a.en.lg" is used
            result = generator.Generate("myTemplate");
            Assert.Equal("content with id: 1.lg from source: abc", result);
        }

        private static (string content, string id) ConstantResolver(string sourceId, string resourceId)
        {
            return ($"# myTemplate\r\n - content with id: {resourceId} from source: {sourceId}", sourceId + resourceId);
        }
    }
}
