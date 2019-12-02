// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests.Skills
{
    public class SkillConversationIdFactoryTests
    {
        [Fact]
        public async Task TestSkillConversationEncoding()
        {
            var conversationId = Guid.NewGuid().ToString("N");
            var serviceUrl = "http://test.com/xyz?id=1&id=2";
            var sc = new TestConversationIdFactory();
            var skillConversationId = await sc.CreateSkillConversationIdAsync(conversationId, serviceUrl, CancellationToken.None);
            var (returnedConversationId, returnedServerUrl) = await sc.GetConversationInfoAsync(skillConversationId, CancellationToken.None);

            Assert.Equal(conversationId, returnedConversationId);
            Assert.Equal(serviceUrl, returnedServerUrl);
        }

        private class TestConversationIdFactory
            : ISkillConversationIdFactory
        {
            public Task<string> CreateSkillConversationIdAsync(string callerConversationId, string serviceUrl, CancellationToken cancellationToken)
            {
                var jsonString = JsonConvert.SerializeObject(new[]
                {
                    callerConversationId,
                    serviceUrl
                });

                return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString)));
            }

            public Task<(string, string)> GetConversationInfoAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(skillConversationId));
                var parts = JsonConvert.DeserializeObject<string[]>(jsonString);
                return Task.FromResult((parts[0], parts[1]));
            }
        }
    }
}
