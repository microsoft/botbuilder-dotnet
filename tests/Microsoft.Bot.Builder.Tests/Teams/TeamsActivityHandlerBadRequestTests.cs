// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    [TestClass]
    public class TeamsActivityHandlerBadRequestTests
    {
        [TestMethod]
        public async Task TestFileConsentBadAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "fileConsent/invoke",
                Value = JObject.FromObject(new FileConsentCardResponse
                {
                    Action = "this.is.a.bad.action",
                    UploadInfo = new FileUploadInfo
                    {
                        UniqueId = "uniqueId",
                        FileType = "fileType",
                        UploadUrl = "uploadUrl",
                    },
                }),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TeamsActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(400, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }

        [TestMethod]
        public async Task TestMessagingExtensionSubmitActionPreviewBadAction()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                Value = JObject.FromObject(new MessagingExtensionAction
                {
                    BotMessagePreviewAction = "this.is.a.bad.action",
                }),
            };

            Activity[] activitiesToSend = null;
            void CaptureSend(Activity[] arg)
            {
                activitiesToSend = arg;
            }

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), activity);

            // Act
            var bot = new TeamsActivityHandler();
            await ((IBot)bot).OnTurnAsync(turnContext);

            // Assert
            Assert.IsNotNull(activitiesToSend);
            Assert.AreEqual(1, activitiesToSend.Length);
            Assert.IsInstanceOfType(activitiesToSend[0].Value, typeof(InvokeResponse));
            Assert.AreEqual(400, ((InvokeResponse)activitiesToSend[0].Value).Status);
        }
    }
}
