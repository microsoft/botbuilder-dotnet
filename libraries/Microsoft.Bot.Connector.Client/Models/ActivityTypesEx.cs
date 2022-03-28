// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// Additional values for ActivityTypes beyond the auto-generated ActivityTypes class.
    /// </summary>
    public static class ActivityTypesEx
    {
        /// <summary>
        /// The type value for delay activities.
        /// </summary>
        /// <remarks>As an outgoing activity type, causes the adapter to pause for <see cref="Activity.Value"/> milliseconds.
        /// The activity's <see cref="Activity.Value"/> should be a <see cref="int"/>.</remarks>
        public const string Delay = "delay";

        /// <summary>
        /// The type value for invoke response activities.
        /// </summary>
        /// <remarks>This is used for a return payload in response to an invoke activity.
        /// Invoke activities communicate programmatic information from a client or channel to a bot, and
        /// have a corresponding return payload for use within the channel. The meaning of an invoke activity
        /// is defined by the <see cref="Activity.Name"/> field, which is meaningful within the scope of a channel.
        /// </remarks>
        public const string InvokeResponse = "invokeResponse";

        /// <summary>
        /// The type value for command activities.
        /// </summary>
        public const string Command = "command";

        /// <summary>
        /// The type value for command result activities.
        /// </summary>
        public const string CommandResult = "commandResult";
    }
}
