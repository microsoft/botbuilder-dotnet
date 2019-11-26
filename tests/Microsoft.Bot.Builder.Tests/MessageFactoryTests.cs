// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Message")]
    public class MessageFactoryTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void NullText()
        {
            var message = MessageFactory.Text(null);
            
            Assert.IsNull(message.Text, "Message Text is not null. Null must have been passed through.");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
        }

        [TestMethod]
        public void TextOnly()
        {
            var messageText = Guid.NewGuid().ToString();

            var message = MessageFactory.Text(messageText);

            Assert.AreEqual(message.Text, messageText, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
        }

        [TestMethod]
        public void TextAndSSML()
        {
            var messageText = Guid.NewGuid().ToString();
            var ssml = @"
                <speak xmlns=""http://www.w3.org/2001/10/synthesis""
                       xmlns:dc=""http://purl.org/dc/elements/1.1/""
                       version=""1.0"">
                  <p>
                    <s xml:lang=""en-US"">
                      <voice name=""Bot"" gender=""neutral"" age=""2"">
                        Bots are <emphasis>Awesome</emphasis>.
                      </voice>
                    </s>
                  </p>
                </speak>";

            var message = MessageFactory.Text(messageText, ssml);

            Assert.AreEqual(message.Text, messageText, "Message Text is not an empty string");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.AreEqual(message.InputHint, InputHints.AcceptingInput, "InputHint is not AcceptingInput");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
        }

        [TestMethod]
        public void SuggestedActionText()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;
            var textActions = new List<string> { "one", "two" };

            var message = MessageFactory.SuggestedActions(textActions, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsNotNull(message.SuggestedActions);
            Assert.IsNotNull(message.SuggestedActions.Actions);
            Assert.IsTrue(message.SuggestedActions.Actions.Count == 2);
            Assert.IsTrue((string)message.SuggestedActions.Actions[0].Value == "one");
            Assert.IsTrue(message.SuggestedActions.Actions[0].Title == "one");
            Assert.IsTrue(message.SuggestedActions.Actions[0].Type == ActionTypes.ImBack);
            Assert.IsTrue((string)message.SuggestedActions.Actions[1].Value == "two");
            Assert.IsTrue(message.SuggestedActions.Actions[1].Title == "two");
            Assert.IsTrue(message.SuggestedActions.Actions[1].Type == ActionTypes.ImBack);
        }

        [TestMethod]
        public void SuggestedActionEnumerable()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;
            var textActions = new HashSet<string> { "one", "two", "three" };

            var message = MessageFactory.SuggestedActions(textActions, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsNotNull(message.SuggestedActions);
            Assert.IsNotNull(message.SuggestedActions.Actions);
            Assert.IsTrue(
                textActions.SetEquals(message.SuggestedActions.Actions.Select(action => (string)action.Value)),
                "The message's suggested actions have the wrong set of values.");
            Assert.IsTrue(
                textActions.SetEquals(message.SuggestedActions.Actions.Select(action => action.Title)),
                "The message's suggested actions have the wrong set of titles.");
            Assert.IsTrue(
                message.SuggestedActions.Actions.All(action => action.Type.Equals(ActionTypes.ImBack)),
                "The message's suggested actions are of the wrong action type.");
        }

        [TestMethod]
        public void SuggestedActionCardAction()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;

            var cardActionValue = Guid.NewGuid().ToString();
            var cardActionTitle = Guid.NewGuid().ToString();

            var ca = new CardAction
            {
                Type = ActionTypes.ImBack,
                Value = cardActionValue,
                Title = cardActionTitle,
            };

            var cardActions = new List<CardAction> { ca };

            var message = MessageFactory.SuggestedActions(cardActions, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsNotNull(message.SuggestedActions);
            Assert.IsNotNull(message.SuggestedActions.Actions);
            Assert.IsTrue(message.SuggestedActions.Actions.Count == 1);
            Assert.IsTrue((string)message.SuggestedActions.Actions[0].Value == cardActionValue);
            Assert.IsTrue(message.SuggestedActions.Actions[0].Title == cardActionTitle);
            Assert.IsTrue(message.SuggestedActions.Actions[0].Type == ActionTypes.ImBack);
        }

        [TestMethod]
        public void SuggestedActionCardActionUnordered()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;

            var cardValue1 = Guid.NewGuid().ToString();
            var cardTitle1 = Guid.NewGuid().ToString();

            var cardAction1 = new CardAction
            {
                Type = ActionTypes.ImBack,
                Value = cardValue1,
                Title = cardTitle1,
            };

            var cardValue2 = Guid.NewGuid().ToString();
            var cardTitle2 = Guid.NewGuid().ToString();

            var cardAction2 = new CardAction
            {
                Type = ActionTypes.ImBack,
                Value = cardValue2,
                Title = cardTitle2,
            };

            var cardActions = new HashSet<CardAction> { cardAction1, cardAction2 };
            var values = new HashSet<object> { cardValue1, cardValue2 };
            var titles = new HashSet<string> { cardTitle1, cardTitle2 };

            var message = MessageFactory.SuggestedActions(cardActions, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsNotNull(message.SuggestedActions);
            Assert.IsNotNull(message.SuggestedActions.Actions);
            Assert.IsTrue(message.SuggestedActions.Actions.Count == 2);
            Assert.IsTrue(
                values.SetEquals(message.SuggestedActions.Actions.Select(action => action.Value)),
                "The message's suggested actions have the wrong set of values.");
            Assert.IsTrue(
                titles.SetEquals(message.SuggestedActions.Actions.Select(action => action.Title)),
                "The message's suggested actions have the wrong set of titles.");
            Assert.IsTrue(
                message.SuggestedActions.Actions.All(action => action.Type.Equals(ActionTypes.ImBack)),
                "The message's suggested actions are of the wrong action type.");
        }

        [TestMethod]
        public void AttachmentSingle()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;

            var attachmentName = Guid.NewGuid().ToString();
            var attachment = new Attachment
            {
                Name = attachmentName,
            };

            var message = MessageFactory.Attachment(attachment, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsTrue(message.Attachments.Count == 1, "Incorrect Attachment Count");
            Assert.IsTrue(message.Attachments[0].Name == attachmentName, "Incorrect Attachment Name");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SuggestedActionsCardActionCollectionNull()
        {
            MessageFactory.SuggestedActions((IEnumerable<CardAction>)null);
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SuggestedActionsStringCollectionNull()
        {
            MessageFactory.SuggestedActions((IEnumerable<string>)null);
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AttachmentNull()
        {
            MessageFactory.Attachment((Attachment)null);
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AttachmentMultipleNull()
        {
            MessageFactory.Attachment((IList<Attachment>)null);
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CarouselNull()
        {
            MessageFactory.Carousel((IList<Attachment>)null);
            Assert.Fail("Exception not thrown");
        }

        [DataTestMethod]
        [DataRow("url", null)]
        [DataRow("url", "")]
        [DataRow("url", " ")]
        [DataRow(null, "contentType")]
        [DataRow("", "contentType")]
        [DataRow(" ", "contentType")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContentUrlNull(string url, string contentType)
        {
            MessageFactory.ContentUrl(url, contentType);
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        public void CarouselTwoAttachments()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;

            var attachmentName = Guid.NewGuid().ToString();
            var attachment1 = new Attachment
            {
                Name = attachmentName,
            };

            var attachmentName2 = Guid.NewGuid().ToString();
            var attachment2 = new Attachment
            {
                Name = attachmentName2,
            };

            var multipleAttachments = new List<Attachment> { attachment1, attachment2 };

            var message = MessageFactory.Carousel(multipleAttachments, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsTrue(message.AttachmentLayout == AttachmentLayoutTypes.Carousel);
            Assert.IsTrue(message.Attachments.Count == 2, "Incorrect Attachment Count");
            Assert.IsTrue(message.Attachments[0].Name == attachmentName, "Incorrect Attachment1 Name");
            Assert.IsTrue(message.Attachments[1].Name == attachmentName2, "Incorrect Attachment2 Name");
        }

        public void CarouselUnorderedAttachments()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;

            var attachmentName1 = Guid.NewGuid().ToString();
            var attachment1 = new Attachment
            {
                Name = attachmentName1,
            };

            var attachmentName2 = Guid.NewGuid().ToString();
            var attachment2 = new Attachment
            {
                Name = attachmentName2,
            };

            var multipleAttachments = new HashSet<Attachment> { attachment1, attachment2 };
            var names = new HashSet<string> { attachmentName1, attachmentName2 };

            var message = MessageFactory.Carousel(multipleAttachments, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsTrue(message.AttachmentLayout == AttachmentLayoutTypes.Carousel);
            Assert.IsTrue(message.Attachments.Count == 2, "Incorrect Attachment Count");
            Assert.IsTrue(names.SetEquals(message.Attachments.Select(a => a.Name)), "Incorrect set of attachment names.");
        }

        [TestMethod]
        public void AttachmentMultiple()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;

            var attachmentName1 = Guid.NewGuid().ToString();
            var attachment1 = new Attachment
            {
                Name = attachmentName1,
            };

            var attachmentName2 = Guid.NewGuid().ToString();
            var attachment2 = new Attachment
            {
                Name = attachmentName2,
            };

            var multipleAttachments = new List<Attachment> { attachment1, attachment2 };

            var message = MessageFactory.Attachment(multipleAttachments, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsTrue(message.AttachmentLayout == AttachmentLayoutTypes.List);
            Assert.IsTrue(message.Attachments.Count == 2, "Incorrect Attachment Count");
            Assert.IsTrue(message.Attachments[0].Name == attachmentName1, "Incorrect Attachment1 Name");
            Assert.IsTrue(message.Attachments[1].Name == attachmentName2, "Incorrect Attachment2 Name");
        }

        [TestMethod]
        public void AttachmentMultipleUnordered()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;

            var attachmentName1 = Guid.NewGuid().ToString();
            var attachment1 = new Attachment
            {
                Name = attachmentName1,
            };

            var attachmentName2 = Guid.NewGuid().ToString();
            var attachment2 = new Attachment
            {
                Name = attachmentName2,
            };

            var multipleAttachments = new HashSet<Attachment> { attachment1, attachment2 };
            var names = new HashSet<string> { attachmentName1, attachmentName2 };

            var message = MessageFactory.Attachment(multipleAttachments, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsTrue(message.AttachmentLayout == AttachmentLayoutTypes.List);
            Assert.IsTrue(message.Attachments.Count == 2, "Incorrect Attachment Count");
            Assert.IsTrue(names.SetEquals(message.Attachments.Select(a => a.Name)), "Incorrect set of attachment names.");
        }

        [TestMethod]
        public void ContentUrl()
        {
            var text = Guid.NewGuid().ToString();
            var ssml = Guid.NewGuid().ToString();
            var inputHint = InputHints.ExpectingInput;
            var uri = $"https://{Guid.NewGuid().ToString()}";
            var contentType = MediaTypeNames.Image.Jpeg;
            var name = Guid.NewGuid().ToString();

            var message = MessageFactory.ContentUrl(uri, contentType, name, text, ssml, inputHint);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.AreEqual(message.InputHint, inputHint, "InputHint does not match");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.IsTrue(message.Attachments.Count == 1);
            Assert.IsTrue(message.Attachments[0].Name == name, "Incorrect Attachment1 Name");
            Assert.IsTrue(message.Attachments[0].ContentType == contentType, "Incorrect contentType");
            Assert.IsTrue(message.Attachments[0].ContentUrl == uri, "Incorrect Uri");
        }

        [DataTestMethod]
        [DataRow("", null)]
        [DataRow("Select color", "Select color")]
        public async Task ValidateImBack(string inputText, string expectedText)
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));

            async Task ReplyWithImBack(ITurnContext ctx, CancellationToken cancellationToken)
            {
                if (ctx.Activity.AsMessageActivity().Text == "test")
                {
                    var activity = MessageFactory.SuggestedActions(
                        new CardAction[]
                        {
                            new CardAction(type: "imBack", text: "red", title: "redTitle"),
                        },
                        inputText);

                    await ctx.SendActivityAsync((Activity)activity);
                }
            }

            void ValidateImBack(IActivity activity)
            {
                Assert.IsTrue(activity.Type == ActivityTypes.Message);

                var messageActivity = activity.AsMessageActivity();

                Assert.IsTrue(messageActivity.Text == expectedText);
                Assert.IsTrue(messageActivity.SuggestedActions.Actions.Count == 1, "Incorrect Count");
                Assert.IsTrue(messageActivity.SuggestedActions.Actions[0].Type == ActionTypes.ImBack, "Incorrect Action Type");
                Assert.IsTrue(messageActivity.SuggestedActions.Actions[0].Text == "red", "incorrect text");
                Assert.IsTrue(messageActivity.SuggestedActions.Actions[0].Title == "redTitle", "incorrect text");
            }

            await new TestFlow(adapter, ReplyWithImBack)
                .Send("test")
                .AssertReply(ValidateImBack, "ImBack Did not validate")
                .StartTestAsync();
        }
    }
}
