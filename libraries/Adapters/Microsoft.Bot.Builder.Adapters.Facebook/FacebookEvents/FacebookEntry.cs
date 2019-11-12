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
        /// Gets or sets the messaging list.
        /// </summary>
        /// <value>List containing one messaging object. Note that even though this is an enumerable, it will only contain one object.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "it needs to be set in ActivityToFacebook method")]
        public List<FacebookMessage> Messaging { get; set; }

        /// <summary>
        /// Gets or sets the changes list.
        /// </summary>
        /// <value>List containing the list of changes.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "it needs to be set in ActivityToFacebook method")]
        public List<FacebookMessage> Changes { get; set; }

        /// <summary>
        /// Gets or sets the standby messages list.
        /// </summary>
        /// <value>List containing the messages sent while in standby mode.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "it needs to be set in ActivityToFacebook method")]
        public List<FacebookMessage> Standby { get; set; }
    }
}
