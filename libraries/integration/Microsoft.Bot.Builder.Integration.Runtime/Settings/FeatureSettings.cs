// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.Runtime.Settings
{
    /// <summary>
    /// Settings for runtime features.
    /// </summary>
    internal class FeatureSettings
    {
        /// <summary>
        /// Gets the configuration key for <see cref="FeatureSettings"/>.
        /// </summary>
        /// <value>
        /// Configuration key for <see cref="FeatureSettings"/>.
        /// </value>
        public static string FeaturesSettingsKey => $"{ConfigurationConstants.RuntimeSettingsKey}:features";

        /// <summary>
        /// Gets or sets a value indicating whether the runtime should remove recipient mentions.
        /// </summary>
        /// <value>
        /// A value indicating whether the runtime should remove recipient mentions.
        /// </value>
        public bool RemoveRecipientMentions { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the runtime should send typing activities.
        /// </summary>
        /// <value>
        /// A value indicating whether the runtime should send typing activities.
        /// </value>
        public bool ShowTyping { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to use inspection middleware.
        /// </summary>
        /// <value>
        /// A value indicating whether to use inspection middleware.
        /// </value>
        public bool UseInspection { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to use traces for transcripts.
        /// </summary>
        /// <value>
        /// A value indicating whether to use traces for transcripts.
        /// </value>
        public bool TraceTranscript { get; set; } = false;

        /// <summary>
        /// Gets or sets the blob transcript store settings.
        /// </summary>
        /// <value>
        /// The blob transcript store settings.
        /// </value>
        public BlobsStorageSettings BlobTranscript { get; set; }

        /// <summary>
        /// Gets or sets the SetSpeakMiddleware settings.
        /// </summary>
        /// <value>
        /// The SetSpeakMiddleware settings.
        /// </value>
        public SpeakSettings SetSpeak { get; set; }
    }
}
