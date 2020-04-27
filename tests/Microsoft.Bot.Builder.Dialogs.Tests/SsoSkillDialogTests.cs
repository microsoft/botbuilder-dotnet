using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Builder.Dialogs.Choices.Channel;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class SsoSkillDialogTests
    {
        [TestMethod]
        public async Task ShouldNotInterceptOAuthCardsForCertainConditions()
        {
            string connectionName = "connectionName";
            var firstResponse = new ExpectedReplies(new List<Activity>() { CreateOAuthCardAttachmentActivity("https://test") });
            var mockSkillClient = new Mock<BotFrameworkClient>();
            mockSkillClient
                .SetupSequence(x => x.PostActivityAsync<ExpectedReplies>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = 200, Body = firstResponse }))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = 200 }));

            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = SkillDialogTests.CreateSkillDialogOptions(conversationState, mockSkillClient);

            var sut = new SkillDialog(dialogOptions);
            var activityToSend = (Activity)Activity.CreateMessageActivity();
            activityToSend.DeliveryMode = DeliveryModes.ExpectReplies;
            activityToSend.Text = Guid.NewGuid().ToString();
            var testAdapter = new TestAdapter(Connector.Channels.Test)
                .Use(new AutoSaveStateMiddleware(conversationState));
            var client = new DialogTestClient(testAdapter, sut, new BeginSkillDialogOptions { Activity = activityToSend, ConnectionName = connectionName }, conversationState: conversationState);
            testAdapter.AddExchangeableToken(connectionName, Channels.Test, "user1", "https://test", "https://test1");
            var finalActivity = await client.SendActivityAsync<IMessageActivity>("irrelevant");
        }

        [TestMethod]
        public void ShouldInterceptOAuthCardsForCertainConditions()
        {
        }

        private static Activity CreateOAuthCardAttachmentActivity(string uri)
        {
            var oauthCard = new OAuthCard()
            {
                TokenExchangeResource = new TokenExchangeResource()
                {
                    Uri = uri
                }
            };
            var attachment = new Attachment()
            {
                ContentType = OAuthCard.ContentType,
                Content = JObject.FromObject(oauthCard)
            };

            var attachmentActivity = MessageFactory.Attachment(attachment);
            attachmentActivity.Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString() };
            attachmentActivity.From = new ChannelAccount("blah", "name");

            return attachmentActivity as Activity;
        }
    }
}
