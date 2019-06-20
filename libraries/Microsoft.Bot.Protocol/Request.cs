using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol
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
