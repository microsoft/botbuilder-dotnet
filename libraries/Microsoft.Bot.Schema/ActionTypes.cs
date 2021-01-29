// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines values for ActionTypes.
    /// </summary>
    public static class ActionTypes
    {
        /// <summary>
        /// The type value for open url actions.
        /// </summary>
        public const string OpenUrl = "openUrl";

        /// <summary>
        /// The type value for I'm Back actions.
        /// </summary>
        public const string ImBack = "imBack";

        /// <summary>
        /// The type value for post back actions.
        /// </summary>
        public const string PostBack = "postBack";

        /// <summary>
        /// The type value for play audio actions.
        /// </summary>
        public const string PlayAudio = "playAudio";

        /// <summary>
        /// The type value for play video actions.
        /// </summary>
        public const string PlayVideo = "playVideo";

        /// <summary>
        /// The type value for show image actions.
        /// </summary>
        public const string ShowImage = "showImage";

        /// <summary>
        /// The type value for download file actions.
        /// </summary>
        public const string DownloadFile = "downloadFile";

        /// <summary>
        /// The type value for sign in actions.
        /// </summary>
        public const string Signin = "signin";

        /// <summary>
        /// The type value for call actions.
        /// </summary>
        public const string Call = "call";

        /// <summary>
        /// The type value for payment actions.
        /// </summary>
        [Obsolete("Bot Framework no longer supports payments.")]
        public const string Payment = "payment";

        /// <summary>
        /// The type value for message back actions.
        /// </summary>
        public const string MessageBack = "messageBack";

        /// <summary>
        /// The type value for open app actions.
        /// </summary>
#pragma warning disable SA1303 // Const field names should begin with upper-case letter (cannot change without breaking backward compatibility)
        public const string openApp = "openApp";
#pragma warning restore SA1303 // Const field names should begin with upper-case letter
    }
}
