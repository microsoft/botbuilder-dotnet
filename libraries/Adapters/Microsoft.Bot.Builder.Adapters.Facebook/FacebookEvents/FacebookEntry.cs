// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookEntry
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>The page ID of the page.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the time of the update.
        /// </summary>
        /// <value>Time of update (epoch time in milliseconds).</value>
        public long Time { get; set; }

        /// <summary>
        /// Gets the messaging list.
        /// </summary>
        /// <value>List containing one messaging object. Note that even though this is an enumerable, it will only contain one object.</value>
        public List<FacebookMessage> Messaging { get; } = new List<FacebookMessage>();

        /// <summary>
        /// Gets the changes list.
        /// </summary>
        /// <value>List containing the list of changes.</value>
        public List<FacebookMessage> Changes { get; } = new List<FacebookMessage>();

        /// <summary>
        /// Gets the standby messages list.
        /// </summary>
        /// <value>List containing the messages sent while in standby mode.</value>
        public List<FacebookMessage> Standby { get; } = new List<FacebookMessage>();

        public bool ShouldSerializeMessaging()
        {
            return Messaging.Count > 0;
        }

        public bool ShouldSerializeStandby()
        {
            return Messaging.Count > 0;
        }

        public bool ShouldSerializeChanges()
        {
            return Messaging.Count > 0;
        }
    }
}
