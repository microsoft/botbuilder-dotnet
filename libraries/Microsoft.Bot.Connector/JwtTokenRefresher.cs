using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public class JwtTokenRefresher : DelegatingHandler
    {
        private readonly MicrosoftAppCredentials credentials;

        public JwtTokenRefresher(MicrosoftAppCredentials credentials)
            : base()
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }
            this.credentials = credentials;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // possibly a transient "token expiration" failure
            // work around for channels that might return Forbidden if the JwtToken is expired
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                response.Dispose();
                // this call might throw if the Microsoft login service returns an oauth failure
                var token = await credentials.GetTokenAsync(true).ConfigureAwait(false);
                // adds token to outgoing request
                await credentials.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
                // retry request with refreshed token
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            // since we've ruled out transient "token expiration" failure, or we've found a permanent Forbidden failure
            // this failure will come from a downstream system like channel connector or state service rather than the Microsoft login service
            // then throw an exception with additional context
            // this centralizes the handling for this StatusCode here rather than the autorest-generated clients
            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            {
                using (response)
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception error)
                    {
                        var statusCode = response.StatusCode;
                        var reasonPhrase = response.ReasonPhrase;
                        throw new UnauthorizedAccessException($"Authorization for Microsoft App ID {credentials.MicrosoftAppId} failed with status code {statusCode} and reason phrase '{reasonPhrase}'", error);
                    }
                }
            }

            return response;
        }
    }
    
    /// <summary>
    /// A custom redirect handler for <see cref="HttpStatusCode.RedirectKeepVerb"/>.
    /// </summary>
    /// <remarks>
    /// This makes sure that authorization headers stay intact between 307 redirects.
    /// </remarks>
    public sealed class CustomRedirectHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if(response.StatusCode == HttpStatusCode.RedirectKeepVerb && response.Headers.Contains("Location"))
            {
                request.RequestUri = new Uri(request.RequestUri, response.Headers.Location);
                response.Dispose();
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            return response;
        }
    }
}
