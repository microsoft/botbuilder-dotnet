// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class AdaptiveDialogBotTests
    {
        [Fact]
        public async Task AdaptiveDialogBotTurnState()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();

            var resourceExplorerMock = new Mock<ResourceExplorer>();

            Resource resource = null;
            resourceExplorerMock.Setup((re) => re.TryGetResource(It.IsAny<string>(), out resource));
                
            //    .Returns(true);

            resourceExplorerMock.Setup((re) => re.LoadTypeAsync<AdaptiveDialog>(It.IsAny<Resource>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AdaptiveDialog("main"));

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();
            var adapterMock = new Mock<BotAdapter>();

            var turnContext = new TurnContext(adapterMock.Object, new Activity());

            // Act
            var bot = new AdaptiveDialogBot(configuration, logger, resourceExplorerMock.Object, storage, botFrameworkClientMock.Object);
            await ((IBot)bot).OnTurnAsync(turnContext, CancellationToken.None);

            // Assert
            Assert.NotNull(turnContext.TurnState.Get<BotFrameworkClient>());
            Assert.NotNull(turnContext.TurnState.Get<SkillConversationIdFactoryBase>());
            Assert.NotNull(turnContext.TurnState.Get<ConversationState>());
            Assert.NotNull(turnContext.TurnState.Get<UserState>());
            Assert.NotNull(turnContext.TurnState.Get<ResourceExplorer>());
            Assert.NotNull(turnContext.TurnState.Get<LanguageGenerator>());
            Assert.NotNull(turnContext.TurnState.Get<LanguageGeneratorManager>());
            Assert.NotNull(turnContext.TurnState.Get<LanguagePolicy>());
        }
    }
}
