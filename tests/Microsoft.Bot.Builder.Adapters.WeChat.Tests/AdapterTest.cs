using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions;
using Microsoft.Bot.Builder.Adapters.WeChat.Test.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test
{
    [TestClass]
    public class AdapterTest
    {
        private WeChatHttpAdapter testAdapter;
        private WeChatHttpAdapter testAdapterUseTempMedia;

        public AdapterTest()
        {
            testAdapter = new WeChatHttpAdapter(MockDataUtility.MockConfiguration(), backgroundService: new QueuedHostedService());
            testAdapterUseTempMedia = new WeChatHttpAdapter(MockDataUtility.MockConfiguration(false), backgroundService: new QueuedHostedService());
        }

        [TestMethod]
        public async Task WeChatHttpAdapterTest()
        {
            var request = CreateMockRequest(MockDataUtility.XmlEncrypt).Object;
            var response = CreateMockResponse().Object;
            var secretInfo = MockDataUtility.GetMockSecretInfo();
            var bot = new EchoBot();
            await testAdapter.ProcessAsync(request, response, bot, secretInfo, true);
            await testAdapter.ProcessAsync(request, response, bot, secretInfo, false);
            await testAdapterUseTempMedia.ProcessAsync(request, response, bot, secretInfo, false);
        }

        private static Mock<HttpRequest> CreateMockRequest(object body)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            var json = body as string ?? JsonConvert.SerializeObject(body);

            sw.Write(json);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);
            var mockHeaders = new HeaderDictionary();
            mockHeaders.Add("Content-Type", "text/xml");
            mockRequest.Setup(x => x.Headers).Returns(mockHeaders);

            return mockRequest;
        }

        private static Mock<HttpResponse> CreateMockResponse()
        {
            return new Mock<HttpResponse>();
        }
    }
}
