using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions
{
    public abstract class RequestHandler
    {
        public abstract Task<Response> ProcessRequestAsync(ReceiveRequest request, object context = null, ILogger<RequestHandler> logger = null);
    }
}
