// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookEntry
    {
        public string Id { get; set; }

        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the messaging list.
        /// </summary>
        /// <value>List containing one messaging object. Note that even though this is an aggregate, it will only contain one messaging object.</value>
        public List<FacebookMessage> Messaging { get; set; }

        public List<FacebookMessage> Changes { get; set; } // TODO: check the type of this list when we have data

        public List<FacebookMessage> Standby { get; set; } // TODO: check the type of this list when we have data
    }
}
