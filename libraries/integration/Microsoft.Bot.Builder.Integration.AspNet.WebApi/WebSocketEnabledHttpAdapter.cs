// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.StreamingExtensions
{
    public class WebSocketEnabledHttpAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private readonly BotFrameworkHttpAdapter _botFrameworkHttpAdapter;
        private readonly WebSocketConnector _webSocketConnector;
        private readonly object initLock = new object();
        private readonly List<IMiddleware> _middlewares = new List<IMiddleware>();
        private Lazy<bool> _ensureMiddlewareSet;

        public WebSocketEnabledHttpAdapter(ICredentialProvider credentialProvider, IChannelProvider channelProvider = null, ILoggerFactory loggerFactory = null)
        {
            if (credentialProvider == null)
            {
                throw new ArgumentNullException(nameof(credentialProvider));
            }

            _botFrameworkHttpAdapter = new BotFrameworkHttpAdapter(credentialProvider, channelProvider, loggerFactory?.CreateLogger<BotFrameworkHttpAdapter>());
            _webSocketConnector = new WebSocketConnector(credentialProvider, channelProvider, loggerFactory?.CreateLogger<WebSocketConnector>());
            _ensureMiddlewareSet = new Lazy<bool>(() =>
            {
                _middlewares.ForEach(mw => _botFrameworkHttpAdapter.Use(mw));
                _botFrameworkHttpAdapter.OnTurnError = OnTurnError;
                return true;
            });
        }

        public new Func<ITurnContext, Exception, Task> OnTurnError { get; set; }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task ProcessAsync(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (httpRequest.Method == HttpMethod.Get && HttpContext.Current.IsWebSocketRequest)
            {
                await _webSocketConnector.ProcessAsync(OnTurnError, _middlewares, httpRequest, httpResponse, bot, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                bool ensure = _ensureMiddlewareSet.Value;
                await _botFrameworkHttpAdapter.ProcessAsync(httpRequest, httpResponse, bot, cancellationToken).ConfigureAwait(false);
            }
        }

        public new WebSocketEnabledHttpAdapter Use(Builder.IMiddleware middleware)
        {
            lock (initLock)
            {
                _middlewares.Add(middleware);
            }

            return this;
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
