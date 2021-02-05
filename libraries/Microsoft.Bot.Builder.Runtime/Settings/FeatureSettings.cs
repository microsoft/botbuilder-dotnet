// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class FeatureSettings
    {
        public bool RemoveRecipientMentions { get; set; } = false;

        public bool ShowTyping { get; set; } = false;

        public bool UseInspection { get; set; } = false;

        public bool TraceTranscript { get; set; } = false;

        public bool BlobTranscript { get; set; } = false;
    }
}
