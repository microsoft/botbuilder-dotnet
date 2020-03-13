using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class SelectorTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(SelectorTests)), monitorChanges: false);
        }

        [TestMethod]
        public async Task SelectorTests_FirstSelector()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task SelectorTests_RandomSelector()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task SelectorTests_MostSpecificFirstSelector()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task SelectorTests_MostSpecificRandomSelector()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task SelectorTests_AdaptiveTrueSelector()
        {
            // only execute first selection
            await TestUtils.RunTestScript(ResourceExplorer, resourceId: "SelectorTests_TrueSelector.test.dialog");
        }

        [TestMethod]
        public async Task SelectorTests_AdaptiveConditionalSelector()
        {
            await TestUtils.RunTestScript(ResourceExplorer, resourceId: "SelectorTests_ConditionalSelector.test.dialog");
        }

        [TestMethod]
        public async Task SelectorTests_RunOnce()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task SelectorTests_Priority()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
