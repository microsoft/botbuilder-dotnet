using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookResponseEvent
    {
        public string Object { get; set; }

        public List<FacebookEntry> Entry { get; set; }
    }
}
