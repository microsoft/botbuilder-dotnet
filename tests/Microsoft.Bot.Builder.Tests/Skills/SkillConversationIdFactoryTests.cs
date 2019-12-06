// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests.Skills
{
    [TestClass]
    public class SkillConversationIdFactoryTests
    {
        [TestMethod]
        public void NullStorageThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new SkillConversationIdFactory(null));
        }

        [TestMethod]
        [DataRow(null, null)]
        [DataRow("", null)]
        [DataRow("notNull", null)]
        [DataRow("notNull", "")]
        public async Task CreateConversationIdValidatesParameters(string conversationId, string serviceUrl)
        {
            var storage = new MemoryStorage();
            var sut = new SkillConversationIdFactory(storage);
            var inputConversationRef = new ConversationReference
            {
                Conversation = new ConversationAccount(id: conversationId),
                ServiceUrl = serviceUrl
            };
            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () => await sut.CreateSkillConversationIdAsync(inputConversationRef, CancellationToken.None));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task GetConversationInfoValidatesParameters(string skillConversationId)
        {
            var storage = new MemoryStorage();
            var sut = new SkillConversationIdFactory(storage);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await sut.GetConversationReferenceAsync(skillConversationId, CancellationToken.None));
        }

        [TestMethod]
        public async Task ReturnsNullIfConversationIdNotFound()
        {
            var storage = new MemoryStorage();
            var sut = new SkillConversationIdFactory(storage);
            var conversationReference = await sut.GetConversationReferenceAsync("Not there", CancellationToken.None);
            Assert.IsNull(conversationReference);
        }

        [TestMethod]
        public async Task CreateAndRetrieveConversationId()
        {
            var storage = new MemoryStorage();
            var sut = new SkillConversationIdFactory(storage);
            var conversationId = Guid.NewGuid().ToString("n");
            var serviceUrl = "http://contoso.com/test";

            // Create
            var inputConversationRef = new ConversationReference
            {
                Conversation = new ConversationAccount(id: conversationId),
                ServiceUrl = serviceUrl,
                ChannelId = Channels.Test
            };

            var skillConversationId = await sut.CreateSkillConversationIdAsync(inputConversationRef, CancellationToken.None);
            Assert.IsNotNull(skillConversationId);

            // Retrieve
            var returnedConversationRef = await sut.GetConversationReferenceAsync(skillConversationId, CancellationToken.None);
            Assert.AreEqual(conversationId, returnedConversationRef.Conversation.Id);
            Assert.AreEqual(Channels.Test, returnedConversationRef.ChannelId);
            Assert.AreEqual(serviceUrl, returnedConversationRef.ServiceUrl);
        }

        [TestMethod]
        public async Task TestSkillConversationEncoding()
        {
            var conversationId = Guid.NewGuid().ToString("N");
            var serviceUrl = "http://test.com/xyz?id=1&id=2";
            var sc = new TestConversationIdFactory();
            var inputRef = new ConversationReference
            {
                Conversation = new ConversationAccount(id: conversationId),
                ServiceUrl = serviceUrl
            };
            var skillConversationId = await sc.CreateSkillConversationIdAsync(inputRef, CancellationToken.None);
            var returnedRef = await sc.GetConversationReferenceAsync(skillConversationId, CancellationToken.None);

            Assert.AreEqual(conversationId, returnedRef.Conversation.Id);
            Assert.AreEqual(serviceUrl, returnedRef.ServiceUrl);
        }

        private class TestConversationIdFactory
            : SkillConversationIdFactoryBase
        {
            private readonly ConcurrentDictionary<string, string> _conversationRefs = new ConcurrentDictionary<string, string>();

            public override Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken)
            {
                var crJson = JsonConvert.SerializeObject(conversationReference);
                var key = (conversationReference.Conversation.Id + conversationReference.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
                _conversationRefs.GetOrAdd(key, crJson);
                return Task.FromResult(key);
            }

            public override Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                var conversationReference = JsonConvert.DeserializeObject<ConversationReference>(_conversationRefs[skillConversationId]);
                return Task.FromResult(conversationReference);
            }

            public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
