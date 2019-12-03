// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Skills.Tests
{
    public class SkillConversationIdFactoryTests
    {
        [Fact]
        public void NullStorageThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SkillConversationIdFactory(null));
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("notNull", null)]
        [InlineData("notNull", "")]
        public async Task CreateConversationIdValidatesParameters(string conversationId, string serviceUrl)
        {
            var storage = new MemoryStorage();
            var sut = new SkillConversationIdFactory(storage);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.CreateSkillConversationIdAsync(conversationId, serviceUrl, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetConversationInfoValidatesParameters(string skillConversationId)
        {
            var storage = new MemoryStorage();
            var sut = new SkillConversationIdFactory(storage);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.GetConversationInfoAsync(skillConversationId, CancellationToken.None));
        }

        [Fact]
        public async Task FailsIfConversationIdNotFound()
        {
            var storage = new MemoryStorage();
            var sut = new SkillConversationIdFactory(storage);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GetConversationInfoAsync("Not there", CancellationToken.None));
        }

        [Fact]
        public async Task CreateAndRetrieveConversationId()
        {
            var storage = new MemoryStorage();
            var sut = new SkillConversationIdFactory(storage);
            var conversationId = Guid.NewGuid().ToString("n");
            var serviceUrl = "http://contoso.com/test";

            // Create
            var skillConversationId = await sut.CreateSkillConversationIdAsync(conversationId, serviceUrl, CancellationToken.None);
            Assert.NotNull(skillConversationId);

            // Retrieve
            var conversationInfo = await sut.GetConversationInfoAsync(skillConversationId, CancellationToken.None);
            Assert.Equal(conversationId, conversationInfo.conversationId);
            Assert.Equal(serviceUrl, conversationInfo.serviceUrl);
        }
    }
}
