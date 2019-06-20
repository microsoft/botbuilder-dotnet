using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.StreamingExtensions.StreamingExtensions.NetCore
{
    public class WebSocketEnabledHttpAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private readonly BotFrameworkHttpAdapter _botFrameworkHttpAdapter;
        private readonly WebSocketConnector _webSocketConnector;
        private readonly object initLock = new object();

        private readonly List<Builder.IMiddleware> middlewares = new List<Builder.IMiddleware>();
        private Lazy<bool> _ensureMiddlewareSet;

        //Note: these constructor overloads exist so that BotFrameworkHttpAdapter can transparently be replaced with WebSocketEnabledHttpAdapter
        public WebSocketEnabledHttpAdapter(IConfiguration configuration, ICredentialProvider credentialProvider = null, IChannelProvider channelProvider = null, ILoggerFactory loggerFactory = null)
            : this()
        {
            if (credentialProvider != null)
            {
                _botFrameworkHttpAdapter = new BotFrameworkHttpAdapter(credentialProvider, channelProvider, loggerFactory?.CreateLogger<BotFrameworkHttpAdapter>());
                _webSocketConnector = new WebSocketConnector(credentialProvider, channelProvider, loggerFactory?.CreateLogger<WebSocketConnector>());
            }
            else //No Credential provider is provided. Create everything from the global configuration.
            {
                _botFrameworkHttpAdapter = new BotFrameworkHttpAdapter(configuration, loggerFactory?.CreateLogger<BotFrameworkHttpAdapter>());
                var credentialProviderPvt = new ConfigurationCredentialProvider(configuration);
                var channelProviderPvt = new ConfigurationChannelProvider(configuration);
                _webSocketConnector = new WebSocketConnector(credentialProviderPvt, channelProviderPvt, loggerFactory?.CreateLogger<WebSocketConnector>());
            }
        }

        private WebSocketEnabledHttpAdapter()
        {
            _ensureMiddlewareSet = new Lazy<bool>(() =>
            {
                middlewares.ForEach(mw => _botFrameworkHttpAdapter.Use(mw));
                _botFrameworkHttpAdapter.OnTurnError = OnTurnError;
                return true;
            });
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (HttpMethods.IsGet(httpRequest.Method) && httpRequest.HttpContext.WebSockets.IsWebSocketRequest)
            {

                await _webSocketConnector.ProcessAsync(OnTurnError, middlewares, httpRequest, httpResponse, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                bool _ = _ensureMiddlewareSet.Value;
                await _botFrameworkHttpAdapter.ProcessAsync(httpRequest, httpResponse, bot, cancellationToken).ConfigureAwait(false);
            }
        }

        public new WebSocketEnabledHttpAdapter Use(Builder.IMiddleware middleware)
        {
            lock (initLock)
            {
                middlewares.Add(middleware);
            }
            return this;
        }

        public new Func<ITurnContext, Exception, Task> OnTurnError { get; set; }

        #region Not Implemented Here
        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
