// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class AdaptiveDialogBotTests
    {
        [Fact]
        public async Task AdaptiveDialogBotTurnState()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceProvider.Add("main.dialog", new MockResource("{ \"$kind\": \"Microsoft.AdaptiveDialog\" }"));
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            // Act
            var bot = new AdaptiveDialogBot(
                "main.dialog", 
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger);
            
            await bot.OnTurnAsync(turnContext, CancellationToken.None);

            // Assert
            Assert.NotNull(turnContext.TurnState.Get<BotFrameworkClient>());
            Assert.NotNull(turnContext.TurnState.Get<SkillConversationIdFactoryBase>());
            Assert.NotNull(turnContext.TurnState.Get<ConversationState>());
            Assert.NotNull(turnContext.TurnState.Get<UserState>());
            Assert.NotNull(turnContext.TurnState.Get<ResourceExplorer>());
            Assert.NotNull(turnContext.TurnState.Get<LanguageGenerator>());
            Assert.NotNull(turnContext.TurnState.Get<LanguageGeneratorManager>());
            Assert.NotNull(turnContext.TurnState.Get<LanguagePolicy>());
            Assert.NotNull(turnContext.TurnState.Get<IBotTelemetryClient>());

            Assert.Null(turnContext.TurnState.Get<MemoryScope>());
            Assert.Null(turnContext.TurnState.Get<Memory.IPathResolver>());
            Assert.Null(turnContext.TurnState.Get<Dialog>());

            // Assert no TestOptions
            var testOptionsAccessor = conversationState.CreateProperty<JObject>("TestOptions");
            Assert.Null(await testOptionsAccessor.GetAsync(turnContext));
        }

        [Fact]
        public async Task AdaptiveDialogBotSetTestOptions()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceProvider.Add("main.dialog", new MockResource("{ \"$kind\": \"Microsoft.AdaptiveDialog\" }"));
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // Type "event" and Name of "SetTestOptions" should store Value in ConversationState.
            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                Type = "event",
                Name = "SetTestOptions",
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" },
                Value = new JObject { { "randomSeed", new JValue(123) }, { "randomValue", new JValue(456) } }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            // Act
            var bot = new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger);

            await bot.OnTurnAsync(turnContext, CancellationToken.None);

            // Assert TestOptions are in Conversation
            var testOptionsAccessor = conversationState.CreateProperty<JObject>("TestOptions");
            Assert.Equal(123, (await testOptionsAccessor.GetAsync(turnContext)).GetValue("randomSeed"));
            Assert.Equal(456, (await testOptionsAccessor.GetAsync(turnContext)).GetValue("randomValue"));
        }

        [Fact]
        public async Task AdaptiveDialogBotExceptionWhenNoResource()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            // Act
            var bot = new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger);

            await Assert.ThrowsAsync<InvalidOperationException>(() => ((IBot)bot).OnTurnAsync(turnContext, CancellationToken.None));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullAdaptiveDialogId()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                null,
                "main.lg",
                resourceExplorer, 
                conversationState, 
                userState, 
                skillConversationIdFactory, 
                languagePolicy, 
                botFrameworkAuthenticationMock.Object, 
                telemetryClient, 
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullLanguageGeneratorId()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                "main.dialog",
                null,
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullResourceExplorer()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                null,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullConversationState()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                null,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullUserState()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                null,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullSkillConversationIdFactoryBase()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                null,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullLanguagePolicy()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                null,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullBotFrameworkAuthentication()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                null,
                telemetryClient,
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldThrowOnNullTelemetryClient()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            Assert.Throws<ArgumentNullException>(() => new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                null,
                logger: logger));
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldNotThrowOnNullMemoryScope()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                scopes: null);
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldNotThrowOnNullPathResolver()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                pathResolvers: null);
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldNotThrowOnNullDialog()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                dialogs: null);
        }

        [Fact]
        public void AdaptiveDialogBot_ShouldNotThrowOnNullLogger()
        {
            // Arrange
            var logger = NullLogger<AdaptiveDialogBot>.Instance;

            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var userState = new UserState(storage);
            var skillConversationIdFactory = new SkillConversationIdFactory(storage);
            var languagePolicy = new LanguagePolicy("en-US");

            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MockResourceProvider(resourceExplorer);
            resourceExplorer.AddResourceProvider(resourceProvider);

            var botFrameworkClientMock = new Mock<BotFrameworkClient>();

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(bfa => bfa.CreateBotFrameworkClient()).Returns(botFrameworkClientMock.Object);

            // The test dialog being used here happens to not send anything so we only need to mock the type.
            var adapterMock = new Mock<BotAdapter>();

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var turnContext = new TurnContext(adapterMock.Object, activity);

            var telemetryClient = new NullBotTelemetryClient();

            new AdaptiveDialogBot(
                "main.dialog",
                "main.lg",
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactory,
                languagePolicy,
                botFrameworkAuthenticationMock.Object,
                telemetryClient,
                logger: null);
        }

        private class MockResourceProvider : ResourceProvider
        {
            private IDictionary<string, Resource> _resources = new Dictionary<string, Resource>();

            public MockResourceProvider(ResourceExplorer resourceExplorer)
                : base(resourceExplorer)
            {
            }

            public override IEnumerable<Resource> GetResources(string extension) => Enumerable.Empty<Resource>();

            public override void Refresh()
            {
            }

            public override bool TryGetResource(string id, out Resource resource) => _resources.TryGetValue(id, out resource);

            public void Add(string id, Resource resource) => _resources.Add(id, resource);
        }

        private class MockResource : Resource
        {
            private string _json;

            public MockResource(string json)
            {
                _json = json;
            }

            public override Task<Stream> OpenStreamAsync() => throw new NotImplementedException();

            public override Task<string> ReadTextAsync() => Task.FromResult(_json);
        }
    }
}
