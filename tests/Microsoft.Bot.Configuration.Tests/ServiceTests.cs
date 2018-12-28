using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Configuration.Tests
{
    [TestClass]
    public class ServiceTests
    {
        [TestMethod]
        public void LuisReturnsCorrectUrl()
        {
            var luis = new LuisService() { Region = "westus" };
            Assert.AreEqual(luis.GetEndpoint(), "https://westus.api.cognitive.microsoft.com");

            luis = new LuisService() { Region = "virginia" };
            Assert.AreEqual(luis.GetEndpoint(), "https://virginia.api.cognitive.microsoft.us");

            luis = new LuisService() { Region = "usgovvirginia" };
            Assert.AreEqual(luis.GetEndpoint(), "https://virginia.api.cognitive.microsoft.us");

            luis = new LuisService() { Region = "usgoviowa" };
            Assert.AreEqual(luis.GetEndpoint(), "https://usgoviowa.api.cognitive.microsoft.us");
        }


        [TestMethod]
        public void QnaMakerSuffixTest()
        {
            var qnamaker = new QnAMakerService() { Hostname = "http://foo.azurewebsites.net" };
            Assert.AreEqual("http://foo.azurewebsites.net/qnamaker", qnamaker.Hostname);

            qnamaker = new QnAMakerService() { Hostname = "http://foo.azurewebsites.net/asdf?x=15" };
            Assert.AreEqual("http://foo.azurewebsites.net/qnamaker", qnamaker.Hostname);

            qnamaker = JsonConvert.DeserializeObject<QnAMakerService>("{\"hostname\":\"http://foo.azurewebsites.net/asdf?x=15\"}");
            Assert.AreEqual("http://foo.azurewebsites.net/qnamaker", qnamaker.Hostname);
        }
    }
}
