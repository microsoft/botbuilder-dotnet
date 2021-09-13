// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines values for DeliveryModes. The deliveryMode signals to the
    /// recipient alternate delivery paths for the activity or response. 
    /// The default value is normal.
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
        /// Activities with a deliveryMode of expectReplies differ in their 
        /// requirement to return a response payload back to the caller 
        /// synchronously, as a direct response to the initial request.
        /// </summary>
        public const string ExpectReplies = "expectReplies";

        /// <summary>
        /// The value for ephemeral delivery modes.
        /// </summary>
        public const string Ephemeral = "ephemeral";
    }
}
