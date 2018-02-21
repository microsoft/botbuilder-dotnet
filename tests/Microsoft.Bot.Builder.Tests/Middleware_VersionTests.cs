using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Microsoft.Bot.Builder.Middleware;
using System.Reflection;
using static Microsoft.Bot.Builder.Middleware.MiddlewareSet;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class Middleware_VersionTests
    {
        
        [TestMethod]
        public async Task MiddlewareVersion_Assembly_Exception()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Middleware_VersionTests));
            int assemblyMajorVersionNumber = assembly.GetName().Version.Major;
            int expectedVersion = assemblyMajorVersionNumber - 1;

            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState((assemblyMajorVersionNumber - 1)));

            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.AreEqual(assemblyMajorVersionNumber - 1, version);
                return Task.CompletedTask;
            };

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(assembly, handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        [TestMethod]
        public async Task MiddlewareVersion_Assembly_NoPreExisting()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Middleware_VersionTests));
            int assemblyMajorVersionNumber = assembly.GetName().Version.Major;

            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState(null));

            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            };

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(assembly, handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        [TestMethod]
        public async Task MiddlewareVersion_Assembly()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Middleware_VersionTests));
            int assemblyMajorVersionNumber = assembly.GetName().Version.Major;

            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState((assemblyMajorVersionNumber)));

            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            };

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(assembly, handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        [TestMethod]
        public async Task MiddlewareVersion_String_Exception()
        {
            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState(13));

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();
            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.AreEqual(13, version);
                return Task.CompletedTask;
            };

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(14, handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        [TestMethod]
        public async Task MiddlewareVersion_String_NoPreExisting()
        {
            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState(null));

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();

            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            };

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(14, handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        [TestMethod]
        public async Task MiddlewareVersion_String()
        {
            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState(14));

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();
            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            };

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(14, handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        [TestMethod]
        public async Task MiddlewareVersion_Empty_Exception()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Middleware_VersionTests));
            int assemblyMajorVersionNumber = assembly.GetName().Version.Major;

            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState((assemblyMajorVersionNumber - 1)));
            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.AreEqual(assemblyMajorVersionNumber - 1, version);
                return Task.CompletedTask;
            };

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }


        [TestMethod]
        public async Task MiddlewareVersion_Empty_NoPreExisting()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Middleware_VersionTests));
            int assemblyMajorVersionNumber = assembly.GetName().Version.Major;

            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState(null));

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();
            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            };

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        [TestMethod]
        public async Task MiddlewareVersion_Empty()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Middleware_VersionTests));

            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            contextMock.Setup(s => s.State).Returns(GetBotState(null));

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();

            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            };

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        [TestMethod]
        public async Task MiddlewareVersion_NullState()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Middleware_VersionTests));

            Mock<IBotContext> contextMock = new Mock<IBotContext>();
            BotState botState = new BotState()
            {
                Conversation = new ConversationState()
            };
            contextMock.Setup(s => s.State).Returns(botState);

            Mock<NextDelegate> nextDelegateMock = new Mock<NextDelegate>();

            Func<IBotContext, int, NextDelegate, Task> handler = (context, version, next) =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            };

            ConversationVersionMiddleware versionMiddleware = new ConversationVersionMiddleware(handler);
            await versionMiddleware.ReceiveActivity(contextMock.Object, nextDelegateMock.Object);
        }

        private BotState GetBotState(int? majorVersion)
        {
            ConversationState conversationState = new ConversationState();
            conversationState[ConversationVersionMiddleware.CONVERSATION_VERSION] = majorVersion;

            BotState botState = new BotState();
            botState.Conversation = conversationState;

            return botState;    
        }

    }
}
