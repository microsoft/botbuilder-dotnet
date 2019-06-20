using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    public class DisconnectedEventArgs : EventArgs
    {
        public string Reason { get; set; }

        public new static DisconnectedEventArgs Empty = new DisconnectedEventArgs();
    }
}
