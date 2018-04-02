using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder.Adapters
{
    /// <summary>
    /// ASP Net Core specific BotFrameworkAdapter functions.
    /// </summary>
    public partial class BotFrameworkAdapter
    {
        /// <summary>
        /// The HTTP context accessor used to access current HttpContext.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkAdapter"/> class.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to use. Use <see cref="MiddlewareSet" class to register multiple middlewares together./></param>
        public BotFrameworkAdapter(
            ICredentialProvider credentialProvider,
            IHttpContextAccessor httpContextAccessor,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient httpClient = null,
            IMiddleware middleware = null)
            : this (credentialProvider, connectorClientRetryPolicy, httpClient, middleware)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the authentication header.
        /// </summary>
        /// <returns>Authentication header if present, null otherwise.</returns>
        public string GetAuthenticationHeader()
        {
            if (this.httpContextAccessor == null)
            {
                throw new ArgumentNullException(
                    nameof(IHttpContextAccessor),
                    "HttpContextAccessor could not be found. Call the constructor with IHttpContextAccessor or ensure that it is registered in Dependency Injection container");
            }

            try
            {
                return this.httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            }
            catch
            {
                return null;
            }
        }
    }
}
