using System.Collections.Generic;

namespace Microsoft.Bot.StreamingExtensions
{
    public partial class Response
    {
        /// <summary>
        /// Status - The Response Status
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// List of associated streams
        /// </summary>
        public List<HttpContentStream> Streams { get; set; }
    }
}
