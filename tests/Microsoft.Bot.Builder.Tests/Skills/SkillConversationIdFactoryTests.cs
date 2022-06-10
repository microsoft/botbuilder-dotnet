﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests.Skills
{
    public class SkillConversationIdFactoryTests
    {
        private const string ServiceUrl = "http://testbot.com/api/messages";
        private const string SkillId = "skill";

        private readonly SkillConversationIdFactory _skillConversationIdFactory = new SkillConversationIdFactory(new MemoryStorage());
        private readonly string _applicationId = Guid.NewGuid().ToString(format: "N");
        private readonly string _botId = Guid.NewGuid().ToString(format: "N");

        [Fact]
        public async Task SkillConversationIdFactoryHappyPath()
        {
            var conversationReference = BuildConversationReference();

            // Create skill conversation
            var skillConversationId = await _skillConversationIdFactory.CreateSkillConversationIdAsync(
                options: new SkillConversationIdFactoryOptions
                {
                    Activity = BuildMessageActivity(conversationReference),
                    BotFrameworkSkill = this.BuildBotFrameworkSkill(),
                    FromBotId = _botId,
                    FromBotOAuthScope = _botId,
                },
                cancellationToken: CancellationToken.None);
            
            Assert.False(string.IsNullOrEmpty(skillConversationId), "Expected a valid skill conversation ID to be created");

            // Retrieve skill conversation
            var retrievedConversationReference = await _skillConversationIdFactory.GetSkillConversationReferenceAsync(skillConversationId, CancellationToken.None);

            // Delete
            await _skillConversationIdFactory.DeleteConversationReferenceAsync(skillConversationId, CancellationToken.None);

            // Retrieve again
            var deletedConversationReference = await _skillConversationIdFactory.GetSkillConversationReferenceAsync(skillConversationId, CancellationToken.None);

            Assert.NotNull(retrievedConversationReference);
            Assert.NotNull(retrievedConversationReference.ConversationReference);
            Assert.Equal(conversationReference, retrievedConversationReference.ConversationReference, new ConversationReferenceEqualityComparer());
            Assert.Null(deletedConversationReference);
        }

        [Fact]
        public async Task IdIsUniqueEachTime()
        {
            var conversationReference = BuildConversationReference();

            // Create skill conversation
            var firstId = await _skillConversationIdFactory.CreateSkillConversationIdAsync(
                options: new SkillConversationIdFactoryOptions
                {
                    Activity = BuildMessageActivity(conversationReference),
                    BotFrameworkSkill = this.BuildBotFrameworkSkill(),
                    FromBotId = _botId,
                    FromBotOAuthScope = _botId,
                },
                cancellationToken: CancellationToken.None);
            
            var secondId = await _skillConversationIdFactory.CreateSkillConversationIdAsync(
                options: new SkillConversationIdFactoryOptions
                {
                    Activity = BuildMessageActivity(conversationReference),
                    BotFrameworkSkill = this.BuildBotFrameworkSkill(),
                    FromBotId = _botId,
                    FromBotOAuthScope = _botId,
                },
                cancellationToken: CancellationToken.None);

            // Ensure that we get a different conversationId each time we call CreateSkillConversationIdAsync
            Assert.NotEqual(firstId, secondId);
        }

        private static ConversationReference BuildConversationReference()
        {
            return new ConversationReference
            {
                Conversation = new ConversationAccount(id: Guid.NewGuid().ToString("N")),
                ServiceUrl = ServiceUrl
            };
        }

        private static Activity BuildMessageActivity(ConversationReference conversationReference)
        {
            if (conversationReference == null)
            {
                throw new ArgumentNullException(nameof(conversationReference));
            }

            var activity = (Activity)Activity.CreateMessageActivity();
            activity.ApplyConversationReference(conversationReference);

            return activity;
        }

        private BotFrameworkSkill BuildBotFrameworkSkill()
        {
            return new BotFrameworkSkill
            {
                AppId = _applicationId,
                Id = SkillId,
                SkillEndpoint = new Uri(ServiceUrl)
            };
        }

        private class ConversationReferenceEqualityComparer : EqualityComparer<ConversationReference>
        {
            public override bool Equals(ConversationReference x, ConversationReference y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return x.Conversation.Id.Equals(y.Conversation?.Id, StringComparison.Ordinal) && x.ServiceUrl.Equals(y.ServiceUrl, StringComparison.Ordinal);
            }

            public override int GetHashCode(ConversationReference obj)
            {
                return (obj.ServiceUrl.GetHashCode() ^ obj.Conversation.Id.GetHashCode()).GetHashCode();
            }
        }
    }
}
