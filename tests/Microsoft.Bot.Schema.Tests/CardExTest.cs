// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    /// <summary>
    /// Tests to ensure that Card Extension methods work as expected.
    /// </summary>
    public class CardExTest
    {
        /// <summary>
        /// Ensures that the <see cref="HeroCard"/> can be used as an attachment.
        /// </summary>
        [Fact]
        public void HeroCardToAttachmentTest()
        {
            var attachments = new List<Attachment>();

            var heroCard = new HeroCard
            {
                Title = "Hero Card Title",
                Subtitle = "Hero Card Subtitle",
                Text = "Testing Text.",
            };
            heroCard.Buttons.Add(new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework"));

            attachments.Add(heroCard.ToAttachment());

            Assert.NotEmpty(attachments);
            Assert.Equal("application/vnd.microsoft.card.hero", attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="ThumbnailCard"/> can be used as an attachment.
        /// </summary>
        [Fact]
        public void ThumbnailCardToAttachmentTest()
        {
            var attachments = new List<Attachment>();
            var buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") };

            var thumbnailCard = new ThumbnailCard(buttons: buttons)
            {
                Title = "Thumbnail Card Title",
                Subtitle = "Thumbnail Card Subtitle",
                Text = "Testing Text.",
            };

            attachments.Add(thumbnailCard.ToAttachment());

            Assert.NotEmpty(attachments);
            Assert.Equal("application/vnd.microsoft.card.thumbnail", attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="SigninCard"/> can be used as an attachment.
        /// </summary>
        [Fact]
        public void SigninCardToAttachmentTest()
        {
            var attachments = new List<Attachment>();
            var buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign-in", value: "https://login.microsoftonline.com/") };

            var signinCard = new SigninCard(buttons: buttons)
            {
                Text = "Testing Text.",
            };

            attachments.Add(signinCard.ToAttachment());

            Assert.NotEmpty(attachments);
            Assert.Equal("application/vnd.microsoft.card.signin", attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="ReceiptCard"/> can be used as an attachment.
        /// </summary>
        [Fact]
        public void ReceiptCardToAttachmentTest()
        {
            var attachments = new List<Attachment>();
            var facts = new List<Fact> { new Fact("Order Number", "1234"), new Fact("Payment Method", "VISA 5555-****") };
            var items = new List<ReceiptItem>
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
            };
            var buttons = new List<CardAction>
            {
                new CardAction(
                    ActionTypes.OpenUrl,
                    "More information",
                    "https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png",
                    value: "https://azure.microsoft.com/en-us/pricing/"),
            };

            var receiptCard = new ReceiptCard(facts: facts, items: items, buttons: buttons)
            {
                Title = "John Doe",
                Tax = "$ 7.50",
                Total = "$ 90.95",
            };

            attachments.Add(receiptCard.ToAttachment());

            Assert.NotEmpty(attachments);
            Assert.Equal("application/vnd.microsoft.card.receipt", attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="AudioCard"/> can be used as an attachment.
        /// </summary>
        [Fact]
        public void AudioCardToAttachmentTest()
        {
            var attachments = new List<Attachment>();

            var audioCard = new AudioCard
            {
                Title = "Audio Card Title",
                Subtitle = "Audio Card Subtitle",
                Text = "Testing text.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/en/3/3c/SW_-_Empire_Strikes_Back.jpg",
                }
            };
            audioCard.Media.Add(new MediaUrl()
            {
                Url = "http://www.wavlist.com/movies/004/father.wav",
            });
            audioCard.Buttons.Add(new CardAction()
            {
                Title = "Read More",
                Type = ActionTypes.OpenUrl,
                Value = "https://en.wikipedia.org/wiki/The_Empire_Strikes_Back",
            });

            attachments.Add(audioCard.ToAttachment());

            Assert.NotEmpty(attachments);
            Assert.Equal("application/vnd.microsoft.card.audio", attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="VideoCard"/> can be used as an attachment.
        /// </summary>
        [Fact]
        public void VideoCardToAttachmentTest()
        {
            var attachments = new List<Attachment>();
            var media = new List<MediaUrl>
            {
                new MediaUrl()
                {
                    Url = "http://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4",
                },
            };
            var buttons = new List<CardAction>
            {
                new CardAction()
                {
                    Title = "Learn More",
                    Type = ActionTypes.OpenUrl,
                    Value = "https://peach.blender.org/",
                },
            };

            var videoCard = new VideoCard(media: media, buttons: buttons)
            {
                Title = "Audio Card Title",
                Subtitle = "Audio Card Subtitle",
                Text = "Testing text.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Big_buck_bunny_poster_big.jpg/220px-Big_buck_bunny_poster_big.jpg",
                }
            };

            attachments.Add(videoCard.ToAttachment());

            Assert.NotEmpty(attachments);
            Assert.Equal("application/vnd.microsoft.card.video", attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="AnimationCard"/> can be used as an attachment.
        /// </summary>
        [Fact]
        public void AnimationCardToAttachmentTest()
        {
            var attachments = new List<Attachment>();

            var animationCard = new AnimationCard
            {
                Title = "Animation Card Title",
                Subtitle = "Animation Card SubtTitle",
                Image = new ThumbnailUrl
                {
                    Url = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png",
                }
            };
            animationCard.Media.Add(new MediaUrl()
            {
                Url = "http://i.giphy.com/Ki55RUbOV5njy.gif",
            });

            attachments.Add(animationCard.ToAttachment());

            Assert.NotEmpty(attachments);
            Assert.Equal("application/vnd.microsoft.card.animation", attachments[0].ContentType);
        }

        /// <summary>
        /// Ensures that the <see cref="OAuthCard"/> can be used as an attachment.
        /// </summary>
        [Fact]
        public void OAuthCardToAttachmentTest()
        {
            var attachments = new List<Attachment>();
            var buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Login", value: "https://login.microsoftonline.com/") };

            var oauthCard = new OAuthCard(buttons: buttons)
            {
                Text = "Please, sign in",
                ConnectionName = "testConnection"
            };

            attachments.Add(oauthCard.ToAttachment());

            Assert.NotEmpty(attachments);
            Assert.Equal("application/vnd.microsoft.card.oauth", attachments[0].ContentType);
        }
    }
}
