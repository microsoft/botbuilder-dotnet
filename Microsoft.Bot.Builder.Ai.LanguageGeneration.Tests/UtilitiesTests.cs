using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class UtilitiesTests
    {
        private LanguageGenerationApplication _lgEndpoint;
        private LanguageGenerationOptions _lgOptions;

        [TestInitialize]
        public void TestInitialize()
        {
            var endpointKey = "<ENDPINT_KEY>";
            var lgAppId = "<APP_ID>";
            var endpointUri = "<ENDPINT_URI>";
            _lgEndpoint = new LanguageGenerationApplication(endpointKey, lgAppId, endpointUri);
            _lgOptions = new LanguageGenerationOptions();
        }


        [TestMethod]
        public async Task TestEndToEnd_ModifyActivity_ValidAsync()
        {
            var activity = new Activity();
            activity.Text = "[wPhrase] my friend";

            var lgResolver = new LanguageGenerationResolver(_lgEndpoint, _lgOptions);
            var metaData = new Dictionary<string, object>();
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello my friend", activity.Text);
        }
    }
}
