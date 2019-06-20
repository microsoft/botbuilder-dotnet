using System.Collections.Generic;

namespace Microsoft.Bot.StreamingExtensions
{
    public partial class Request
    {
        /// <summary>
        /// Request verb, null on responses
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// Request path; null on responses
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// List of associated streams
        /// </summary>
        public List<HttpContentStream> Streams { get; set; }
    }
}
