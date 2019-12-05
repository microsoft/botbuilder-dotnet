using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class SelectorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task SelectorTests_FirstSelector()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task SelectorTests_RandomSelector()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task SelectorTests_MostSpecificFirstSelector()
        {
            await TestUtils.RunTestScript();
        }

        [TestMethod]
        public async Task SelectorTests_MostSpecificRandomSelector()
        {
            await TestUtils.RunTestScript();
        }
    }
}
