using System.IO;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    public class StreamWrapper
    {
        public Stream Stream { get; set; }

        public int? StreamLength { get; set; }
    }
}
