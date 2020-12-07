// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Builders.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Handlers
{
    public class OnTurnErrorHandlerBuilderTests
    {
        public static IEnumerable<object[]> GetOnTurnErrorHandlerSucceedsData()
        {
            yield return new object[]
            {
                (ILogger<IBotFrameworkHttpAdapter>)null,
                (ConversationState)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                (ILogger<IBotFrameworkHttpAdapter>)new NullLogger<BotFrameworkHttpAdapter>(),
                (ConversationState)new ConversationState(new MemoryStorage()),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };
        }

        [Fact]
        public void Build_Succeeds()
        {
            IServiceProvider services = new ServiceCollection()
                .BuildServiceProvider();

            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Func<ITurnContext, Exception, Task> onTurnError = new OnTurnErrorHandlerBuilder().Build(services, configuration);

            Assert.NotNull(onTurnError);
            Assert.IsType<Func<ITurnContext, Exception, Task>>(onTurnError);
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
                () => new OnTurnErrorHandlerBuilder().Build(services, configuration));
        }

        [Theory]
        [MemberData(nameof(GetOnTurnErrorHandlerSucceedsData))]
        public async Task OnTurnErrorHandlerSucceeds(
            ILogger<IBotFrameworkHttpAdapter> logger,
            ConversationState conversationState,
            IConfiguration configuration)
        {
            IServiceProvider services = new ServiceCollection()
                .AddTransient<ILogger<IBotFrameworkHttpAdapter>>(_ => logger)
                .AddSingleton<ConversationState>(_ => conversationState)
                .BuildServiceProvider();

            var adapter = new TestAdapter(TestAdapter.CreateConversation("OnTurnErrorHandler"))
            {
                OnTurnError = new OnTurnErrorHandlerBuilder().Build(services, configuration)
            };

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                await Task.CompletedTask;

                throw new Exception("Fake exception");
            })
                .Send("Hi")
                .AssertReply("Fake exception")
                .StartTestAsync();
        }
    }
}
