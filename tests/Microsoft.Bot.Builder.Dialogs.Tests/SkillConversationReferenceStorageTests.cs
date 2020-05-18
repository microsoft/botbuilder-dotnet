// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class SkillConversationReferenceStorageTests
    {
        [TestMethod]
        public void CtorThrowsNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new SkillConversationReferenceStorage(null));
        }

        [TestMethod]
        public async Task StorageIdIsUnique()
        {
            SkillConversationReferenceStorage scr;
            ConversationReference cr;
            SkillConversationIdFactoryOptions options;
            CreateOptions(out scr, out cr, out options);

            var id = await scr.CreateSkillConversationIdAsync(options, default(CancellationToken));
            Assert.IsTrue(!string.IsNullOrEmpty(id));

            var id2 = await scr.CreateSkillConversationIdAsync(options, default(CancellationToken));
            Assert.IsTrue(!string.IsNullOrEmpty(id2));

            Assert.AreNotEqual(id, id2);
        }

        [TestMethod]
        public async Task VerifyCRUD()
        {
            SkillConversationReferenceStorage scr;
            ConversationReference cr;
            SkillConversationIdFactoryOptions options;
            CreateOptions(out scr, out cr, out options);

            var id = await scr.CreateSkillConversationIdAsync(options, CancellationToken.None);
            Assert.IsTrue(!string.IsNullOrEmpty(id));

            var record = await scr.GetSkillConversationReferenceAsync(id, CancellationToken.None);
            Assert.AreEqual(id, record.Id);
            Assert.AreEqual(cr.ServiceUrl, record.ConversationReference.ServiceUrl);
            Assert.AreEqual(cr.Locale, record.ConversationReference.Locale);
            Assert.AreEqual(cr.User.Id, record.ConversationReference.User.Id);
            Assert.AreEqual(cr.Bot.Id, record.ConversationReference.Bot.Id);
            Assert.AreEqual(cr.ChannelId, record.ConversationReference.ChannelId);
            Assert.AreEqual(cr.Conversation.Id, record.ConversationReference.Conversation.Id);
            Assert.IsNull(record.Activity);

            record.Activity = new Activity() { Type = ActivityTypes.EndOfConversation };
            await scr.SaveSkillConversationReferenceAsync(record, CancellationToken.None);

            record = await scr.GetSkillConversationReferenceAsync(id, CancellationToken.None);
            Assert.AreEqual(id, record.Id);
            Assert.AreEqual(cr.ServiceUrl, record.ConversationReference.ServiceUrl);
            Assert.AreEqual(cr.Locale, record.ConversationReference.Locale);
            Assert.AreEqual(cr.User.Id, record.ConversationReference.User.Id);
            Assert.AreEqual(cr.Bot.Id, record.ConversationReference.Bot.Id);
            Assert.AreEqual(cr.ChannelId, record.ConversationReference.ChannelId);
            Assert.AreEqual(cr.Conversation.Id, record.ConversationReference.Conversation.Id);
            Assert.IsNotNull(record.Activity);
            Assert.AreEqual(ActivityTypes.EndOfConversation, record.Activity.Type);

            await scr.DeleteConversationReferenceAsync(id, CancellationToken.None);

            record = await scr.GetSkillConversationReferenceAsync(id, CancellationToken.None);
            Assert.IsNull(record);
        }

        private static void CreateOptions(out SkillConversationReferenceStorage scr, out ConversationReference cr, out SkillConversationIdFactoryOptions options)
        {
            scr = new SkillConversationReferenceStorage(new MemoryStorage());
            var activity = (Activity)Activity.CreateMessageActivity();
            activity.Id = Guid.NewGuid().ToString("n");
            activity.Locale = "en-US";
            activity.ChannelId = "test";
            activity.From = new ChannelAccount(id: "user");
            activity.Recipient = new ChannelAccount(id: "bot");
            activity.Conversation = new ConversationAccount(id: "123123");
            activity.ServiceUrl = "http//mybot.com";
            cr = activity.GetConversationReference();
            var skill = new BotFrameworkSkill()
            {
                AppId = Guid.NewGuid().ToString("n"),
                Id = "skill",
                SkillEndpoint = new Uri("http://testbot.com/api/messages")
            };

            options = new SkillConversationIdFactoryOptions
            {
                FromBotOAuthScope = "mybot",
                FromBotId = "mybot",
                Activity = activity,
                BotFrameworkSkill = skill
            };
        }
    }
}
