// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1402 // File may only contain a single type
using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills.Tests
{
    [TestClass]
    public class SkillConversationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestSkillConversationEncoding()
        {
            var sc = new SkillConversation
            {
                ConversationId = Guid.NewGuid().ToString("N"),
                ServiceUrl = "http://test.com/xyz?id=1&id=2"
            };
            var skillConversationId = sc.GetSkillConversationId();

            var sc2 = new SkillConversation(skillConversationId);
            Assert.AreEqual(sc.ConversationId, sc2.ConversationId);
            Assert.AreEqual(sc.ServiceUrl, sc2.ServiceUrl);
        }

        [TestMethod]
        public void TestSkillConversationTestNullId()
        {
            try
            {
                var sc = new SkillConversation
                {
                    ConversationId = null,
                    ServiceUrl = "http://test.com/xyz?id=1&id=2"
                };
                var cid = sc.GetSkillConversationId();
                Assert.Fail("Should have thrown on null");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void TestSkillConversationNullUrl()
        {
            try
            {
                var sc = new SkillConversation
                {
                    ConversationId = Guid.NewGuid().ToString("N"),
                    ServiceUrl = null
                };
                var cid = sc.GetSkillConversationId();
                Assert.Fail("Should have thrown on null");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void TestSkillConversationNullCtor()
        {
            try
            {
                var sc = new SkillConversation(null);
                var cid = sc.GetSkillConversationId();
                Assert.Fail("Should have thrown on null");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void TestSkillConversationEmptyCtor()
        {
            try
            {
                var sc = new SkillConversation(string.Empty);
                var cid = sc.GetSkillConversationId();
                Assert.Fail("Should have thrown on empty");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void TestSkillConversationBogusPayload()
        {
            try
            {
                var test = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new object[0])));

                var sc = new SkillConversation(test);
                var cid = sc.GetSkillConversationId();
                Assert.Fail("Should have thrown on bogusity");
            }
            catch (Exception)
            {
            }
        }
    }
}
