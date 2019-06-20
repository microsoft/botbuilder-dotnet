using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol
{
    public class ReceiveRequest
    {
        /// <summary>
        /// Request verb, null on responses
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// Request path; null on responses
        /// </summary>
        public string Path { get; set; }

        public List<IContentStream> Streams { get; set; }
    }
}
