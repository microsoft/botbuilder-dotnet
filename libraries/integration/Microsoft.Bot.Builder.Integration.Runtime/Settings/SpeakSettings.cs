// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.Runtime.Settings
{
    /// <summary>
    /// Speak settings for the runtim.  This is used by SetSpeakMiddleware.
    /// </summary>
    public class SpeakSettings
    {
        /// <summary>
        /// Gets or sets the SSML voice name attribute value.
        /// </summary>
        /// <value>
        /// The SSML voice name attribute value.
        /// </value>
        public string VoiceFontName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the behavior to set an empty Activity.Speak 
        /// property with the value from Activity.Text.
        /// </summary>
        /// <value>
        /// true to indicates empty Activity.Speak should be set with Activity.Text.
        /// </value>
        public bool FallbackToTextForSpeechIfEmpty { get; set; }

        /// <summary>
        /// Gets or sets the xml:lang value for a SSML speak element.
        /// </summary>
        /// <value>
        /// The xml:lang value.
        /// </value>
        public string Lang { get; set; }
    }
}
