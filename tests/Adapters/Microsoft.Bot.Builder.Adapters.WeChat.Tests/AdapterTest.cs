// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
{
    public class AdapterTest
    {
        [Fact]
        public async Task WeChatHttpAdapterTest()
        {
            var request = CreateMockRequest(MockDataUtility.XmlEncrypt).Object;
            var response = CreateMockResponse().Object;
            var secretInfo = MockDataUtility.GetMockSecretInfo();
            var storage = new MemoryStorage();
            var taskQueue = new BackgroundTaskQueue();
            var bot = new EchoBot();
            var testAdapter1 = new WeChatHttpAdapter(MockDataUtility.MockWeChatSettings(true, false), storage, taskQueue);
            var testAdapter2 = new WeChatHttpAdapter(MockDataUtility.MockWeChatSettings(false, true), storage, taskQueue);
            var testAdapter3 = new WeChatHttpAdapter(MockDataUtility.MockWeChatSettings(true, true), storage, taskQueue);
            var testAdapter4 = new WeChatHttpAdapter(MockDataUtility.MockWeChatSettings(false, false), storage, taskQueue);

            await testAdapter1.ProcessAsync(request, response, bot, secretInfo);
            await testAdapter2.ProcessAsync(request, response, bot, secretInfo);
            await testAdapter3.ProcessAsync(request, response, bot, secretInfo);
            await testAdapter4.ProcessAsync(request, response, bot, secretInfo);
        }

        private static Mock<HttpRequest> CreateMockRequest(object body)
        {
            var ms = new MemoryStream();

            // Do not dispose writer.
            var sw = new StreamWriter(ms);
            var json = body as string ?? JsonConvert.SerializeObject(body);
            sw.Write(json);
            sw.Flush();
            ms.Position = 0;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);
            var mockHeaders = new HeaderDictionary
            {
                { "Content-Type", "text/xml" },
            };
            mockRequest.Setup(x => x.Headers).Returns(mockHeaders);

            return mockRequest;
        }

        private static Mock<HttpResponse> CreateMockResponse()
        {
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(x => x.Body).Returns(new MemoryStream());
            return mockResponse;
        }
    }
}
