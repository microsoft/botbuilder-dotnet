// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// ThumbnailCard ContentType value.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1402: Maintainability Rules : File may only contain a single type.", Justification = "The card partial classes should be place in the same file to keep the readability of the code.")]
    public partial class ThumbnailCard
    {
        public const string ContentType = "application/vnd.microsoft.card.thumbnail";
    }

    /// <summary>
    /// HeroCard ContentType value.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1402: Maintainability Rules : File may only contain a single type.", Justification = "The card partial classes should be place in the same file to keep the readability of the code.")]
    public partial class HeroCard
    {
        public const string ContentType = "application/vnd.microsoft.card.hero";
    }

    /// <summary>
    /// ReceiptCard ContentType value.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1402: Maintainability Rules : File may only contain a single type.", Justification = "The card partial classes should be place in the same file to keep the readability of the code.")]
    public partial class ReceiptCard
    {
        public const string ContentType = "application/vnd.microsoft.card.receipt";
    }

    /// <summary>
    /// SigninCard ContentType value.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1402: Maintainability Rules : File may only contain a single type.", Justification = "The card partial classes should be place in the same file to keep the readability of the code.")]
    public partial class SigninCard
    {
        public const string ContentType = "application/vnd.microsoft.card.signin";
    }

    /// <summary>
    /// AnimationCard ContentType value.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1402: Maintainability Rules : File may only contain a single type.", Justification = "The card partial classes should be place in the same file to keep the readability of the code.")]
    public partial class AnimationCard
    {
        public const string ContentType = "application/vnd.microsoft.card.animation";
    }

    /// <summary>
    /// AudioCard ContentType value.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1402: Maintainability Rules : File may only contain a single type.", Justification = "The card partial classes should be place in the same file to keep the readability of the code.")]
    public partial class AudioCard
    {
        public const string ContentType = "application/vnd.microsoft.card.audio";
    }

    /// <summary>
    /// VideoCard ContentType value.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1402: Maintainability Rules : File may only contain a single type.", Justification = "The card partial classes should be place in the same file to keep the readability of the code.")]
    public partial class VideoCard
    {
        public const string ContentType = "application/vnd.microsoft.card.video";
    }

    /// <summary>
    /// OAuthCard ContentType value.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1402: Maintainability Rules : File may only contain a single type.", Justification = "The card partial classes should be place in the same file to keep the readability of the code.")]
    public partial class OAuthCard
    {
        public const string ContentType = "application/vnd.microsoft.card.oauth";
    }
}
