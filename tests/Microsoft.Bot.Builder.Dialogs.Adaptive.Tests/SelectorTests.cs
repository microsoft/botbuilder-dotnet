using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class SelectorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task SelectorTests_AdaptiveFirstSelector()
        {
            await TestUtils.RunTestScript("SelectorTests_FirstSelector.test.dialog");
        }

        [TestMethod]
        public async Task SelectorTests_AdaptiveRandomSelector()
        {
            await TestUtils.RunTestScript("SelectorTests_RandomSelector.test.dialog");
        }

        [TestMethod]
        public async Task SelectorTests_MostSpecificFirstSelector()
        {
            await TestUtils.RunTestScript("SelectorTests_MostSpecificFirstSelector.test.dialog");
        }

        [TestMethod]
        public async Task SelectorTests_MostSpecificRandomSelector()
        {
            await TestUtils.RunTestScript("SelectorTests_MostSpecificRandomSelector.test.dialog");
        }

        [TestMethod]
        public async Task SelectorTests_AdaptiveTrueSelector()
        {
            // only execute first selection
            await TestUtils.RunTestScript("SelectorTests_TrueSelector.test.dialog");
        }

        [TestMethod]
        public async Task SelectorTests_AdaptiveConditionalSelector()
        {
            await TestUtils.RunTestScript("SelectorTests_ConditionalSelector.test.dialog");
        }
    }
}
