using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol
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
