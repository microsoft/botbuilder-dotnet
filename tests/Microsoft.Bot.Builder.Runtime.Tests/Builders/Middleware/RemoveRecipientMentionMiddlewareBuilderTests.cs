// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Middleware
{
    public class RemoveRecipientMentionMiddlewareBuilderTests
    {
        [Fact]
        public void Build_Succeeds()
        {
            IServiceProvider services = new ServiceCollection()
                .BuildServiceProvider();

            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            IMiddleware middleware = new RemoveRecipientMentionMiddlewareBuilder().Build(services, configuration);

            Assert.NotNull(middleware);
            Assert.IsType<RemoveRecipientMentionMiddlewareBuilder>(middleware);
        }

        [Theory]
        [MemberData(
            nameof(BuilderTestDataGenerator.GetBuildArgumentNullExceptionData),
            MemberType = typeof(BuilderTestDataGenerator))]
        public void Build_Throws_ArgumentNullException(
            string paramName,
            IServiceProvider services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new RemoveRecipientMentionMiddlewareBuilder().Build(services, configuration));
        }

        /// <summary>
        /// Different channels have their own implementation of how users are mentioned.
        /// Slack uses @username, while Teams uses &lt;at&gt;username&lt;/at&gt;.
        /// </summary>
        /// <param name="mentionedTextValue"> Mentioned entity text value.</param>
        /// <param name="utterance">User utterance.</param>
        /// <param name="receivedUtterance">User utterance received by bot after middleware processing.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("@Bot", "@Bot Hi Bot", "Hi Bot")]
        [InlineData("<at>Bot</at>", "<at>Bot</at> Hi Bot", "Hi Bot")]
        public async Task RemoveAtMention(string mentionedTextValue, string utterance, string receivedUtterance)
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("RemoveAtMention"))
                .Use(new RemoveRecipientMentionMiddlewareBuilder());

            var mention = new Mention
            {
                Mentioned = adapter.Conversation.Bot,
                Text = mentionedTextValue,
                Properties = JObject.FromObject(new
                {
                    mentioned = new
                    {
                        id = adapter.Conversation.Bot.Id,
                        name = adapter.Conversation.Bot.Name
                    },
                    text = mentionedTextValue,
                    type = "mention"
                })
            };

            var mentionActivity = MessageFactory.Text(utterance);
            mentionActivity.Entities = new List<Entity> { mention };

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                Assert.Equal(receivedUtterance, context.Activity.Text);

                await Task.CompletedTask;
            })
                .Send(mentionActivity)
                .StartTestAsync();
        }
    }
}
