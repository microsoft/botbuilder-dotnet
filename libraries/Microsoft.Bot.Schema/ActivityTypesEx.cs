// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Additional values for ActivityTypes beyond the auto-generated ActivityTypes class.
    /// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public static class ActivityTypesEx
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
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
    }
}
