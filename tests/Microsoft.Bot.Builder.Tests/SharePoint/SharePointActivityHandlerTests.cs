// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.SharePoint;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Microsoft.Bot.Builder.SharePoint.Tests
{
    public class SharePointActivityHandlerTests
    {
        [Fact]
        public async Task TestSharePointGetCardViewAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "cardExtension/getCardView",
                Value = JObject.FromObject(new object()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnSharePointTaskGetCardViewAsync", bot.Record[0]);
            Assert.NotNull(activitiesToSend);
            Assert.Single(activitiesToSend);
            Assert.IsType<InvokeResponse>(activitiesToSend[0].Value);
            Assert.Equal(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [Fact]
        public async Task TestSharePointGetQuickViewAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "cardExtension/getQuickView",
                Value = JObject.FromObject(new object()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnSharePointTaskGetQuickViewAsync", bot.Record[0]);
            Assert.NotNull(activitiesToSend);
            Assert.Single(activitiesToSend);
            Assert.IsType<InvokeResponse>(activitiesToSend[0].Value);
            Assert.Equal(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [Fact]
        public async Task TestSharePointGetPropertyPaneConfigurationAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "cardExtension/getPropertyPaneConfiguration",
                Value = JObject.FromObject(new object()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnSharePointTaskGetPropertyPaneConfigurationAsync", bot.Record[0]);
            Assert.NotNull(activitiesToSend);
            Assert.Single(activitiesToSend);
            Assert.IsType<InvokeResponse>(activitiesToSend[0].Value);
            Assert.Equal(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [Fact]
        public async Task TestSharePointSetPropertyPaneConfigurationAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "cardExtension/setPropertyPaneConfiguration",
                Value = JObject.FromObject(new object()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnSharePointTaskSetPropertyPaneConfigurationAsync", bot.Record[0]);
            Assert.NotNull(activitiesToSend);
            Assert.Single(activitiesToSend);
            Assert.IsType<InvokeResponse>(activitiesToSend[0].Value);
            Assert.Equal(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [Fact]
        public async Task TestSharePointHandleActionAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "cardExtension/handleAction",
                Value = JObject.FromObject(new object()),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TestActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.Single(bot.Record);
            Assert.Equal("OnSharePointTaskHandleActionAsync", bot.Record[0]);
            Assert.NotNull(activitiesToSend);
            Assert.Single(activitiesToSend);
            Assert.IsType<InvokeResponse>(activitiesToSend[0].Value);
            Assert.Equal(200, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        private class TestActivityHandler : SharePointActivityHandler
        {
            public List<string> Record { get; } = new List<string>();

            // Invoke
            protected override Task<GetCardViewResponse> OnSharePointTaskGetCardViewAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new GetCardViewResponse(GetCardViewResponse.CardViewTemplateType.PrimaryTextCardView));
            }

            protected override Task<GetPropertyPaneConfigurationResponse> OnSharePointTaskGetPropertyPaneConfigurationAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new GetPropertyPaneConfigurationResponse());
            }

            protected override Task<GetQuickViewResponse> OnSharePointTaskGetQuickViewAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult(new GetQuickViewResponse());
            }

            protected override Task OnSharePointTaskSetPropertyPaneConfigurationAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.CompletedTask;
            }

            protected override Task<BaseHandleActionResponse> OnSharePointTaskHandleActionAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
            {
                Record.Add(MethodBase.GetCurrentMethod().Name);
                return Task.FromResult<BaseHandleActionResponse>(new NoOpHandleActionResponse());
            }
        }
    }
}
