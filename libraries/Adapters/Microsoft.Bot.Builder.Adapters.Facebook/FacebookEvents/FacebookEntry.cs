using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookEntry
    {
        public string Id { get; set; }

        public int Time { get; set; }

        /// <summary>
        /// Gets or sets the messaging list.
        /// </summary>
        /// <value>List containing one messaging object. Note that even though this is an aggregate, it will only contain one messaging object.</value>
        public List<FacebookMessage> Messaging { get; set; }

        public List<FacebookMessage> Changes { get; set; } // TODO: check the type of this list when we have data

        public List<FacebookMessage> Standby { get; set; } // TODO: check the type of this list when we have data
    }
}
