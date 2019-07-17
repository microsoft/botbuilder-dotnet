// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockRequestHandler : RequestHandler
    {
        private readonly Func<ReceiveRequest, StreamingResponse> _responseAction;
        private readonly Func<ReceiveRequest, Task<StreamingResponse>> _responseActionAsync;

        public MockRequestHandler(Func<ReceiveRequest, StreamingResponse> responseAction)
        {
            _responseAction = responseAction;
            _responseActionAsync = null;
        }

        public MockRequestHandler(Func<ReceiveRequest, Task<StreamingResponse>> responseActionAsync)
        {
            _responseActionAsync = responseActionAsync;
            _responseAction = null;
        }

        public override async Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger<RequestHandler> logger, object context = null, CancellationToken cancellationToken = default(CancellationToken))
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
