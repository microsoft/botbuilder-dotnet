// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Represents a Facebook Message Entry.
    /// </summary>
    public class FacebookEntry
    {
        /// <summary>
        /// Gets or sets the page ID.
        /// </summary>
        /// <value>The ID of the page.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the time of the update.
        /// </summary>
        /// <value>The time of update (epoch time in milliseconds).</value>
        public long Time { get; set; }

        /// <summary>
        /// Gets the messaging list.
        /// </summary>
        /// <value>List containing one messaging object.</value>
        /// <remarks>Note that even though this is an enumerable, it will only contain one object.</remarks>
        public List<FacebookMessage> Messaging { get; } = new List<FacebookMessage>();

        /// <summary>
        /// Gets the changes list.
        /// </summary>
        /// <value>The list of changes.</value>
        public List<FacebookMessage> Changes { get; } = new List<FacebookMessage>();

        /// <summary>
        /// Gets the standby messages list.
        /// </summary>
        /// <value>List containing the messages sent while in standby mode.</value>
        public List<FacebookMessage> Standby { get; } = new List<FacebookMessage>();

        /// <summary>
        /// Newtonsoft JSON method for conditionally serializing the <see cref="Messaging"/> property.
        /// </summary>
        /// <returns>`true` to serialize the property; otherwise, `false`.</returns>
        public bool ShouldSerializeMessaging()
        {
            return Messaging.Count > 0;
        }

        /// <summary>
        /// Newtonsoft JSON method for conditionally serializing the <see cref="Standby"/> property.
        /// </summary>
        /// <returns>`true` to serialize the property; otherwise, `false`.</returns>
        public bool ShouldSerializeStandby()
        {
            return Standby.Count > 0;
        }

        /// <summary>
        /// Newtonsoft JSON method for conditionally serializing the <see cref="Changes"/> property.
        /// </summary>
        /// <returns>`true` to serialize the property; otherwise, `false`.</returns>
        public bool ShouldSerializeChanges()
        {
            return Changes.Count > 0;
        }
    }
}
