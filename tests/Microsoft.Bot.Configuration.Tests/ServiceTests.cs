// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Configuration.Tests
{
#pragma warning disable CS0618 // Type or member is obsolete (Excluding so we can test obsolete classes)
    public class ServiceTests
    {
        [Fact]
        public void LuisReturnsCorrectUrl()
        {
            var luis = new LuisService { Region = "westus" };
            Assert.Equal("https://westus.api.cognitive.microsoft.com", luis.GetEndpoint());

            luis = new LuisService { Region = "virginia" };
            Assert.Equal("https://virginia.api.cognitive.microsoft.us", luis.GetEndpoint());

            luis = new LuisService { Region = "usgovvirginia" };
            Assert.Equal("https://virginia.api.cognitive.microsoft.us", luis.GetEndpoint());

            luis = new LuisService { Region = "usgoviowa" };
            Assert.Equal("https://usgoviowa.api.cognitive.microsoft.us", luis.GetEndpoint());
        }

        [Fact]
        public void QnaMakerSuffixTest()
        {
            var qnamaker = new QnAMakerService { Hostname = "http://foo.azurewebsites.net" };
            Assert.Equal("http://foo.azurewebsites.net/qnamaker", qnamaker.Hostname);

            qnamaker = new QnAMakerService { Hostname = "http://foo.azurewebsites.net/asdf?x=15" };
            Assert.Equal("http://foo.azurewebsites.net/qnamaker", qnamaker.Hostname);

            qnamaker = JsonConvert.DeserializeObject<QnAMakerService>("{\"hostname\":\"http://foo.azurewebsites.net/asdf?x=15\"}");
            Assert.Equal("http://foo.azurewebsites.net/qnamaker", qnamaker.Hostname);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
