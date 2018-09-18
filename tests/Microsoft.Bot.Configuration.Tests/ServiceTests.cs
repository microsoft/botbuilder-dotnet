using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        }


        [TestMethod]
        public void QnaMakerSuffixTest()
        {
            var qnamaker = new QnAMakerService() { Hostname = "http://foo.azurewebsites.net" };
            Assert.AreEqual("http://foo.azurewebsites.net/qnamaker", qnamaker.Hostname);

            qnamaker = new QnAMakerService() { Hostname = "http://foo.azurewebsites.net/asdf?x=15" };
            Assert.AreEqual("http://foo.azurewebsites.net/qnamaker", qnamaker.Hostname);
        }
    }
}
