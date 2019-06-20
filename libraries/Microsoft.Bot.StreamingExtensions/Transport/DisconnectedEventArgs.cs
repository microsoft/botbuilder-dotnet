using System;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    public class DisconnectedEventArgs : EventArgs
    {
        public static new DisconnectedEventArgs Empty { get; set; } = new DisconnectedEventArgs();

        public string Reason { get; set; }
    }
}
