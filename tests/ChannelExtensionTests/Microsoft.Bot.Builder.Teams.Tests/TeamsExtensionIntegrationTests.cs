using System;
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    /// <summary>
    /// Tests the integration of Microsoft.Bot.Builder.Teams package.
    /// </summary>
    [TestClass]
    public class TeamsExtensionIntegrationTests
    {
        /// <summary>
        /// Tests the pipeline with a plain test message.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [TestMethod]
        public async Task TestPlainTextMessageRequestPipeline()
        {
            WebHostHelper webHostHelper = WebHostHelper.GetWebHostHelper(
                async (turnContext, cancellationToken) =>
                {
                    // --> Get Teams Extensions.
                    ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();

                    ResourceResponse resourceResponse = await turnContext.SendActivityAsync($"You sent '{turnContext.Activity.Text}'").ConfigureAwait(false);

                    Assert.AreEqual("Test", resourceResponse.Id);
                },
                (httpRequest) =>
                {
                    if (httpRequest.RequestUri.AbsoluteUri == "https://canary.botapi.skype.com/amer/v3/conversations/a%3AConversationId/activities/1550367849687")
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(
                                JsonConvert.SerializeObject(new ResourceResponse
                                {
                                    Id = "Test",
                                }),
                                Encoding.UTF8,
                                "application/json"),
                        };
                    }
                    else
                    {
                        Assert.Fail("Unexpected request uri");
                        throw new InvalidOperationException("Unexpected uri");
                    }
                });

            HttpResponseMessage responseMessage = await webHostHelper.SendRequestAsync(File.ReadAllText(@"Requests\PlainTextMessage.json"));

            Assert.AreEqual(HttpStatusCode.OK, responseMessage.StatusCode);
        }
    }
}
