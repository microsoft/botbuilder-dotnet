using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.LanguageGeneration.Resolver;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        private LGEndpoint _lgEndpoint;
        private LGOptions _lgOptions;

        [TestInitialize]
        public void TestInitialize()
        {
            var endpointKey = "<ENDPINT_KEY>";
            var lgAppId = "<APP_ID>";
            var endpointUri = "<ENDPINT_URI>";
            _lgEndpoint = new LGEndpoint(endpointKey, lgAppId, endpointUri);
            _lgOptions = new LGOptions();
        }


        [TestMethod]
        public async Task TestEndToEnd_ModifyActivity_ValidAsync()
        {
            var activity = new Activity();
            activity.Text = "[wPhrase] my friend";

            var lgResolver = new LGResolver(_lgEndpoint, _lgOptions);
            var metaData = new Dictionary<string, object>();
            await lgResolver.ResolveAsync(activity, metaData).ConfigureAwait(false);

            Assert.AreEqual("Hello my friend", activity.Text);
        }
    }
}
