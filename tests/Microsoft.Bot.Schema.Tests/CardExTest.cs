﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Schema.Tests
{
    /// <summary>
    /// Tests to ensure that Card Extension methods work as expected.
    /// </summary>
    [TestClass]
    public class CardExTest
    {
        private static readonly List<Attachment> _attachments = new List<Attachment>();

        /// <summary>
        /// Ensures that the <see cref="HeroCard"/> can be added as an attachment in a message.
        /// </summary>
        [TestMethod]
        public void HeroCardToAttachmentTest()
        {
            var message = MessageFactory.Attachment(_attachments);

            var heroCard = new HeroCard
            {
                Title = "Hero Card Title",
                Subtitle = "Hero Card Subtitle",
                Text = "Testing Text.",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            message.Attachments.Add(heroCard.ToAttachment());

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual("application/vnd.microsoft.card.hero", message.Attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="ThumbnailCard"/> can be added as an attachment in a message.
        /// </summary>
        [TestMethod]
        public void ThumbnailCardToAttachmentTest()
        {
            var message = MessageFactory.Attachment(_attachments);

            var thumbnailCard = new ThumbnailCard
            {
                Title = "Thumbnail Card Title",
                Subtitle = "Thumbnail Card Subtitle",
                Text = "Testing Text.",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            message.Attachments.Add(thumbnailCard.ToAttachment());

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual("application/vnd.microsoft.card.thumbnail", message.Attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="SigninCard"/> can be added as an attachment in a message.
        /// </summary>
        [TestMethod]
        public void SigninCardToAttachmentTest()
        {
            var message = MessageFactory.Attachment(_attachments);

            var signinCard = new SigninCard
            {
                Text = "Testing Text.",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign-in", value: "https://login.microsoftonline.com/") },
            };

            message.Attachments.Add(signinCard.ToAttachment());

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual("application/vnd.microsoft.card.signin", message.Attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="ReceiptCard"/> can be added as an attachment in a message.
        /// </summary>
        [TestMethod]
        public void ReceiptCardToAttachmentTest()
        {
            var message = MessageFactory.Attachment(_attachments);

            var receiptCard = new ReceiptCard
            {
                Title = "John Doe",
                Facts = new List<Fact> { new Fact("Order Number", "1234"), new Fact("Payment Method", "VISA 5555-****") },
                Items = new List<ReceiptItem>
                {
                    new ReceiptItem(
                        "Data Transfer",
                        price: "$ 38.45",
                        quantity: "368",
                        image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png")),
                    new ReceiptItem(
                        "App Service",
                        price: "$ 45.00",
                        quantity: "720",
                        image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png")),
                },
                Tax = "$ 7.50",
                Total = "$ 90.95",
                Buttons = new List<CardAction>
                {
                    new CardAction(
                        ActionTypes.OpenUrl,
                        "More information",
                        "https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png",
                        value: "https://azure.microsoft.com/en-us/pricing/"),
                },
            };

            message.Attachments.Add(receiptCard.ToAttachment());

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual("application/vnd.microsoft.card.receipt", message.Attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="AudioCard"/> can be added as an attachment in a message.
        /// </summary>
        [TestMethod]
        public void AudioCardToAttachmentTest()
        {
            var message = MessageFactory.Attachment(_attachments);

            var audioCard = new AudioCard
            {
                Title = "Audio Card Title",
                Subtitle = "Audio Card Subtitle",
                Text = "Testing text.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/en/3/3c/SW_-_Empire_Strikes_Back.jpg",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://www.wavlist.com/movies/004/father.wav",
                    },
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Read More",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://en.wikipedia.org/wiki/The_Empire_Strikes_Back",
                    },
                },
            };

            message.Attachments.Add(audioCard.ToAttachment());

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual("application/vnd.microsoft.card.audio", message.Attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="VideoCard"/> can be added as an attachment in a message.
        /// </summary>
        [TestMethod]
        public void VideoCardToAttachmentTest()
        {
            var message = MessageFactory.Attachment(_attachments);

            var videoCard = new VideoCard
            {
                Title = "Audio Card Title",
                Subtitle = "Audio Card Subtitle",
                Text = "Testing text.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Big_buck_bunny_poster_big.jpg/220px-Big_buck_bunny_poster_big.jpg",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4",
                    },
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Learn More",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://peach.blender.org/",
                    },
                },
            };

            message.Attachments.Add(videoCard.ToAttachment());

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual("application/vnd.microsoft.card.video", message.Attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="AnimationCard"/> can be added as an attachment in a message.
        /// </summary>
        [TestMethod]
        public void AnimationCardToAttachmentTest()
        {
            var message = MessageFactory.Attachment(_attachments);

            var animationCard = new AnimationCard
            {
                Title = "Animation Card Title",
                Subtitle = "Animation Card SubtTitle",
                Image = new ThumbnailUrl
                {
                    Url = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://i.giphy.com/Ki55RUbOV5njy.gif",
                    },
                },
            };

            message.Attachments.Add(animationCard.ToAttachment());

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual("application/vnd.microsoft.card.animation", message.Attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="OAuthCard"/> can be added as an attachment in a message.
        /// </summary>
        [TestMethod]
        public void OAuthCardToAttachmentTest()
        {
            var message = MessageFactory.Attachment(_attachments);
            
            var oauthCard = new OAuthCard
            {
                Text = "Please, sign in",
                ConnectionName = "testConnection",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Login", value: "https://login.microsoftonline.com/") },
            };

            message.Attachments.Add(oauthCard.ToAttachment());

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual("application/vnd.microsoft.card.oauth", message.Attachments[0].ContentType);
        }
    }
}
