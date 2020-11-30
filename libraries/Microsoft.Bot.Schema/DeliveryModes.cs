// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines values for DeliveryModes.
    /// </summary>
    public static class DeliveryModes
    {
        /// <summary>
        /// The mode value for normal delivery modes.
        /// </summary>
        public const string Normal = "normal";

        /// <summary>
        /// The mode value for notification delivery modes.
        /// </summary>
        public const string Notification = "notification";

        /// <summary>
        /// The value for expected replies delivery modes.
        /// </summary>
        public const string ExpectReplies = "expectReplies";

        /// <summary>
        /// The value for ephemeral delivery modes.
        /// </summary>
        public const string Ephemeral = "ephemeral";
    }
}
