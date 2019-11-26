// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class SkillConversationIdFactoryTests
    {
        [Fact]
        public void TestSkillConversationEncoding()
        {
            var conversationId = Guid.NewGuid().ToString("N");
            var serviceUrl = "http://test.com/xyz?id=1&id=2";
            var sc = new TestConversationIdFactory();
            var skillConversationId = sc.CreateSkillConversationId(conversationId, serviceUrl);
            var (returnedConversationId, returnedServerUrl) = sc.GetConversationInfo(skillConversationId);

            Assert.Equal(conversationId, returnedConversationId);
            Assert.Equal(serviceUrl, returnedServerUrl);
        }

        private class TestConversationIdFactory
            : ISkillConversationIdFactory
        {
            public string CreateSkillConversationId(string conversationId, string serviceUrl)
            {
                var jsonString = JsonConvert.SerializeObject(new[]
                {
                    conversationId,
                    serviceUrl
                });

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            }

            public (string, string) GetConversationInfo(string conversationId)
            {
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(conversationId));
                var parts = JsonConvert.DeserializeObject<string[]>(jsonString);
                return (parts[0], parts[1]);
            }
        }
    }
}
