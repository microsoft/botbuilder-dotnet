using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LGGeneratorTests
    {

        private string GetFallbackFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "Fallback";
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TestExactLanguageLookup()
        {
            var resourceManager = ResourceExplorer.LoadProject(GetFallbackFolder());
            var lg = new LGLanguageGenerator(resourceManager);

            Assert.AreEqual("english-us", await lg.Generate("en-us", id: "test"));
            Assert.AreEqual("english-gb", await lg.Generate("en-gb", id: "test"));
            Assert.AreEqual("english", await lg.Generate("en", id: "test"));
            Assert.AreEqual("default", await lg.Generate("", id: "test"));
            Assert.AreEqual("default", await lg.Generate("foo", id: "test"));

            Assert.AreEqual("default2", await lg.Generate("en-us", id: "test2"));
            Assert.AreEqual("default2", await lg.Generate("en-gb", id: "test2"));
            Assert.AreEqual("default2", await lg.Generate("en", id: "test2"));
            Assert.AreEqual("default2", await lg.Generate("", id: "test2"));
            Assert.AreEqual("default2", await lg.Generate("foo", id: "test2"));
        }

        [TestMethod]
        public async Task TestInheritenceLookup()
        {
            // inheritence order is z -> y -> x meaning z derives from y derives from x
            string[] zTypes = new string[] { "z", "y", "x" };
            string[] yTypes = new string[] { "y", "x" };
            string[] xTypes = new string[] { "x" };

            var resourceManager = ResourceExplorer.LoadProject(GetFallbackFolder());
            var lg = new LGLanguageGenerator(resourceManager);

            // property is defined at each point in the hierarchy
            Assert.AreEqual("test x", await lg.Generate("en", id: "property", types: xTypes));
            Assert.AreEqual("test y", await lg.Generate("en", id: "property", types: yTypes));
            Assert.AreEqual("test z", await lg.Generate("en", id: "property", types: zTypes));

            // property 2 is only defined at the x level
            Assert.AreEqual("test2 x", await lg.Generate("en", id: "property2", types: xTypes));
            Assert.AreEqual("test2 x", await lg.Generate("en", id: "property2", types: yTypes));
            Assert.AreEqual("test2 x", await lg.Generate("en", id: "property2", types: zTypes));
        }

        [TestMethod]
        public async Task TestTags()
        {
            string[] notags = new string[] { };
            string[] tags1 = new string[] { "tag1" };
            string[] tags2 = new string[] { "tag2" };
            string[] oddTags = new string[] { "foo", "bar" };

            var resourceManager = ResourceExplorer.LoadProject(GetFallbackFolder());
            var lg = new LGLanguageGenerator(resourceManager);

            Assert.AreEqual("english", await lg.Generate("en", id: "test", tags: notags));
            Assert.AreEqual("english", await lg.Generate("en", id: "test", tags: oddTags));
            Assert.AreEqual("tag1 test", await lg.Generate("en", id: "test", tags: tags1));
            Assert.AreEqual("tag2 test", await lg.Generate("en", id: "test", tags: tags2));
        }

        [TestMethod]
        public async Task TestTagsAndInheritence()
        {
            string[] zTypes = new string[] { "z", "y", "x" };
            string[] yTypes = new string[] { "y", "x" };
            string[] xTypes = new string[] { "x" };

            string[] notags = new string[] { };
            string[] tags1 = new string[] { "tag1" };
            string[] tags2 = new string[] { "tag2" };
            string[] oddTags = new string[] { "foo", "bar" };

            var resourceManager = ResourceExplorer.LoadProject(GetFallbackFolder());
            var lg = new LGLanguageGenerator(resourceManager);

            Assert.AreEqual("test x", await lg.Generate("en", id: "property", tags: notags, types: xTypes));
            Assert.AreEqual("test y", await lg.Generate("en", id: "property", tags: notags, types: yTypes));
            Assert.AreEqual("test z", await lg.Generate("en", id: "property", tags: notags, types: zTypes));

            Assert.AreEqual("test x", await lg.Generate("en", id: "property", tags: oddTags, types: xTypes));
            Assert.AreEqual("test y", await lg.Generate("en", id: "property", tags: oddTags, types: yTypes));
            Assert.AreEqual("test z", await lg.Generate("en", id: "property", tags: oddTags, types: zTypes));

            Assert.AreEqual("test x", await lg.Generate("en", id: "property", tags: tags2, types: xTypes));
            Assert.AreEqual("test tag2 y", await lg.Generate("en", id: "property", tags: tags2, types: yTypes));
        }
    }
}
