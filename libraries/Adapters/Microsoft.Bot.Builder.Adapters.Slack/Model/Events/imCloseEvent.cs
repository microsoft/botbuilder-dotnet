using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Events
{
    /// <summary>
    /// Represents an IM Created event https://api.slack.com/events/im_close
    /// </summary>
    public class ImCloseEvent
    {
        public string Type => "im_close";

        public string Channel { get; set; }

        public string User { get; set; }
    }
}
