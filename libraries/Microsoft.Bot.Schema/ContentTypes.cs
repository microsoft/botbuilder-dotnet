// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines values for Cards' ContentTypes.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// The content type value of a <see cref="ThumbnailCard"/>.
        /// </summary>
        public const string ThumbnailCard = "application/vnd.microsoft.card.thumbnail";

        /// <summary>
        /// The content type value of a <see cref="HeroCard"/>.
        /// </summary>
        public const string HeroCard = "application/vnd.microsoft.card.hero";

        /// <summary>
        /// The content type value of a <see cref="ReceiptCard"/>.
        /// </summary>
        public const string ReceiptCard = "application/vnd.microsoft.card.receipt";

        /// <summary>
        /// The content type value of a <see cref="SigninCard"/>.
        /// </summary>
        public const string SigninCard = "application/vnd.microsoft.card.signin";

        /// <summary>
        /// The content type value of a <see cref="AnimationCard"/>.
        /// </summary>
        public const string AnimationCard = "application/vnd.microsoft.card.animation";

        /// <summary>
        /// The content type value of a <see cref="AudioCard"/>.
        /// </summary>
        public const string AudioCard = "application/vnd.microsoft.card.audio";

        /// <summary>
        /// The content type value of a <see cref="VideoCard"/>.
        /// </summary>
        public const string VideoCard = "application/vnd.microsoft.card.video";

        /// <summary>
        /// The content type value of a <see cref="OAuthCard"/>.
        /// </summary>
        public const string OAuthCard = "application/vnd.microsoft.card.oauth";
    }
}
