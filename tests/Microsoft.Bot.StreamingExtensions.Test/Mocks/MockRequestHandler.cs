using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockRequestHandler : RequestHandler
    {
        private readonly Func<ReceiveRequest, Response> _responseAction;
        private readonly Func<ReceiveRequest, Task<Response>> _responseActionAsync;

        public MockRequestHandler(Func<ReceiveRequest, Response> responseAction)
        {
            _responseAction = responseAction;
            _responseActionAsync = null;
        }

        public MockRequestHandler(Func<ReceiveRequest, Task<Response>> responseActionAsync)
        {
            _responseActionAsync = responseActionAsync;
            _responseAction = null;
        }

        public override async Task<Response> ProcessRequestAsync(ReceiveRequest request, object context = null, ILogger<RequestHandler> logger = null)
        {
            if (_responseAction != null)
            {
                return _responseAction(request);
            }
            else
            {
                return await _responseActionAsync(request);
            }
        }
    }
}
