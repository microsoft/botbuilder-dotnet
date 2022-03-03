// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Extension methods for Cards.
    /// </summary>
    public static class CardEx
    {
        /// <summary>
        /// Creates a new attachment from <see cref="HeroCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="HeroCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this HeroCard card)
        {
            return CreateAttachment(card, ContentTypes.HeroCard);
        }

        /// <summary>
        /// Creates a new attachment from <see cref="ThumbnailCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="ThumbnailCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this ThumbnailCard card)
        {
            return CreateAttachment(card, ContentTypes.ThumbnailCard);
        }

        /// <summary>
        /// Creates a new attachment from <see cref="SigninCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="SigninCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this SigninCard card)
        {
            return CreateAttachment(card, ContentTypes.SigninCard);
        }

        /// <summary>
        /// Creates a new attachment from <see cref="ReceiptCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="ReceiptCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this ReceiptCard card)
        {
            return CreateAttachment(card, ContentTypes.ReceiptCard);
        }

        /// <summary>
        /// Creates a new attachment from <see cref="AudioCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="AudioCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this AudioCard card)
        {
            return CreateAttachment(card, ContentTypes.AudioCard);
        }

        /// <summary>
        /// Creates a new attachment from <see cref="VideoCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="VideoCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this VideoCard card)
        {
            return CreateAttachment(card, ContentTypes.VideoCard);
        }

        /// <summary>
        /// Creates a new attachment from <see cref="AnimationCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="AnimationCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this AnimationCard card)
        {
            return CreateAttachment(card, ContentTypes.AnimationCard);
        }

        /// <summary>
        /// Creates a new attachment from <see cref="OAuthCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="OAuthCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this OAuthCard card)
        {
            return CreateAttachment(card, ContentTypes.OAuthCard);
        }

        private static Attachment CreateAttachment<T>(T card, string contentType)
        {
            return new Attachment
            {
                Content = card,
                ContentType = contentType,
            };
        }
    }
}
