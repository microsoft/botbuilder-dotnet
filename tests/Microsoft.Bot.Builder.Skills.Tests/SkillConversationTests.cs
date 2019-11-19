// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Skills.Tests
{
    public class SkillConversationTests
    {
        [Fact]
        public void TestSkillConversationEncoding()
        {
            var sc = new SkillConversation()
            {
                ConversationId = Guid.NewGuid().ToString("N"),
                ServiceUrl = "http://test.com/xyz?id=1&id=2"
            };
            var skillConversationId = sc.GetSkillConversationId();

            var sc2 = new SkillConversation(skillConversationId);
            Assert.Equal(sc.ConversationId, sc2.ConversationId);
            Assert.Equal(sc.ServiceUrl, sc2.ServiceUrl);
        }

        [Fact]
        public void TestSkillConversationTestNullId()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                var sc = new SkillConversation()
                {
                    ConversationId = null,
                    ServiceUrl = "http://test.com/xyz?id=1&id=2"
                };
                var cid = sc.GetSkillConversationId();
            });
        }

        [Fact]
        public void TestSkillConversationNullUrl()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                var sc = new SkillConversation()
                {
                    ConversationId = Guid.NewGuid().ToString("N"),
                    ServiceUrl = null
                };
                var cid = sc.GetSkillConversationId();
            });
        }

        [Fact]
        public void TestSkillConversationNullCtor()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var sc = new SkillConversation(null);
                var cid = sc.GetSkillConversationId();
            });
        }

        [Fact]
        public void TestSkillConversationEmptyCtor()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                var sc = new SkillConversation(string.Empty);
                var cid = sc.GetSkillConversationId();
            });
        }

        [Fact]
        public void TestSkillConversationBogusPayload()
        {
            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                var test = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new object[0])));

                var sc = new SkillConversation(test);
                var cid = sc.GetSkillConversationId();
            });
        }
    }
}
