using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <inheritdoc />
    public class CloudBotFrameworkClient : BotFrameworkHttpClientBase
    {
        private readonly BotFrameworkAuthentication _auth;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBotFrameworkClient"/> class.
        /// </summary>
        /// <param name="httpClient">A <see cref="HttpClient"/>.</param>
        /// <param name="auth">An instance of <see cref="BotFrameworkAuthentication"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public CloudBotFrameworkClient(
            HttpClient httpClient,
            BotFrameworkAuthentication auth,
            ILogger logger = null)
            : base(httpClient, logger)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        /// <inheritdoc />
        protected override async Task<AppCredentials> BuildCredentialsAsync(string appId, string oAuthScope = null)
        {
            return await _auth.GetAppCredentialsAsync(appId, HttpClient, oAuthScope, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override string GetOriginatingAudience()
        {
            return _auth.GetOriginatingAudience();
        }
    }
}
