// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Message")]
    public class MessageFactoryTests
    {
        [TestMethod]
        public void NullText()
        {
            IMessageActivity message = MessageFactory.Text(null);
            Assert.AreEqual(message.Text, string.Empty, "Message Text is not an empty string");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
        }

        [TestMethod]
        public void TextOnly()
        {
            string messageText = Guid.NewGuid().ToString();
            IMessageActivity message = MessageFactory.Text(messageText);
            Assert.AreEqual(message.Text, messageText, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
        }

        [TestMethod]
        public void TextAndSSML()
        {
            string messageText = Guid.NewGuid().ToString();
            string ssml = @"
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
            IMessageActivity message = MessageFactory.Text(messageText, ssml);
            Assert.AreEqual(message.Text, messageText, "Message Text is not an empty string");
            Assert.AreEqual(message.Speak, ssml, "ssml text is incorrect");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
        }

        [TestMethod]
        public void SuggestedActionText()
        {
            string text = Guid.NewGuid().ToString();
            string ssml = Guid.NewGuid().ToString();
            IList<string> textActions = new List<string> { "one", "two" };

            IMessageActivity message = MessageFactory.SuggestedActions(textActions, text, ssml);
            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
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
        public void SuggestedActionCardAction()
        {
            string text = Guid.NewGuid().ToString();
            string ssml = Guid.NewGuid().ToString();

            string cardActionValue = Guid.NewGuid().ToString();
            string cardActionTitle = Guid.NewGuid().ToString();

            CardAction ca = new CardAction
            {
                Type = ActionTypes.ImBack,
                Value = cardActionValue,
                Title = cardActionTitle
            };

            IList<CardAction> cardActions = new List<CardAction> { ca };

            IMessageActivity message = MessageFactory.SuggestedActions(cardActions, text, ssml);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.IsNotNull(message.SuggestedActions);
            Assert.IsNotNull(message.SuggestedActions.Actions);
            Assert.IsTrue(message.SuggestedActions.Actions.Count == 1);
            Assert.IsTrue((string)message.SuggestedActions.Actions[0].Value == cardActionValue);
            Assert.IsTrue(message.SuggestedActions.Actions[0].Title == cardActionTitle);
            Assert.IsTrue(message.SuggestedActions.Actions[0].Type == ActionTypes.ImBack);
        }

        [TestMethod]
        public void AttachmentSingle()
        {
            string text = Guid.NewGuid().ToString();
            string ssml = Guid.NewGuid().ToString();

            string attachmentName = Guid.NewGuid().ToString();
            Attachment a = new Attachment
            {
                Name = attachmentName
            };

            IMessageActivity message = MessageFactory.Attachment(a, text, ssml);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.IsTrue(message.Attachments.Count == 1, "Incorrect Attachment Count");
            Assert.IsTrue(message.Attachments[0].Name == attachmentName, "Incorrect Attachment Name"); 
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AttachmentNull()
        {
            IMessageActivity message = MessageFactory.Attachment((Attachment)null);
            Assert.Fail("Exception not thrown"); 
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AttachmentMultipleNull()
        {
            IMessageActivity message = MessageFactory.Attachment((IList<Attachment>)null);
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CarouselNull()
        {
            IMessageActivity message = MessageFactory.Carousel((IList<Attachment>)null);
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        
        public void CarouselTwoAttachments()
        {
            string text = Guid.NewGuid().ToString();
            string ssml = Guid.NewGuid().ToString();

            string attachmentName = Guid.NewGuid().ToString();
            Attachment attachment1 = new Attachment
            {
                Name = attachmentName
            };

            string attachmentName2 = Guid.NewGuid().ToString();
            Attachment attachment2 = new Attachment
            {
                Name = attachmentName2
            };

            IList<Attachment> multipleAttachments = new List<Attachment> { attachment1, attachment2 };
            IMessageActivity message = MessageFactory.Carousel(multipleAttachments, text, ssml);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.IsTrue(message.AttachmentLayout == AttachmentLayoutTypes.Carousel);
            Assert.IsTrue(message.Attachments.Count == 2, "Incorrect Attachment Count");
            Assert.IsTrue(message.Attachments[0].Name == attachmentName, "Incorrect Attachment1 Name");
            Assert.IsTrue(message.Attachments[1].Name == attachmentName2, "Incorrect Attachment2 Name");
        }


        [TestMethod]
        public void AttachmentMultiple()
        {
            string text = Guid.NewGuid().ToString();
            string ssml = Guid.NewGuid().ToString();

            string attachmentName = Guid.NewGuid().ToString();            
            Attachment a = new Attachment
            {
                Name = attachmentName
            };

            string attachmentName2 = Guid.NewGuid().ToString();
            Attachment a2 = new Attachment
            {
                Name = attachmentName2
            };

            IList<Attachment> multipleAttachments = new List<Attachment> { a, a2 };
            IMessageActivity message = MessageFactory.Attachment(multipleAttachments, text, ssml);

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.IsTrue(message.AttachmentLayout == AttachmentLayoutTypes.List);
            Assert.IsTrue(message.Attachments.Count == 2, "Incorrect Attachment Count");
            Assert.IsTrue(message.Attachments[0].Name == attachmentName, "Incorrect Attachment1 Name");
            Assert.IsTrue(message.Attachments[1].Name == attachmentName2, "Incorrect Attachment2 Name");
        }

        [TestMethod]
        public void ContentUrl()
        {
            string text = Guid.NewGuid().ToString();
            string ssml = Guid.NewGuid().ToString();
            string uri = $"https:// { Guid.NewGuid().ToString()}";
            string contentType = MediaTypeNames.Image.Jpeg;
            string name =  Guid.NewGuid().ToString(); 

            IMessageActivity message = MessageFactory.ContentUrl(uri, contentType, name, text, ssml);            

            Assert.AreEqual(message.Text, text, "Message Text does not match");
            Assert.AreEqual(message.Type, ActivityTypes.Message, "Incorrect Activity Type");
            Assert.IsTrue(message.Attachments.Count == 1);            
            Assert.IsTrue(message.Attachments[0].Name == name, "Incorrect Attachment1 Name");
            Assert.IsTrue(message.Attachments[0].ContentType == contentType, "Incorrect contentType");
            Assert.IsTrue(message.Attachments[0].ContentUrl == uri, "Incorrect Uri");
        }
    }
}
