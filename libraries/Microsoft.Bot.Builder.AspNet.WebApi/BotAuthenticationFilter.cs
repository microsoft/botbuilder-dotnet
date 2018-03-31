// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Microsoft.Bot.Builder.AspNet.WebApi
{
    /// <summary>
    /// Bot authentication filter.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.IAuthorizationFilter" />
    public class BotAuthenticationFilter : AuthorizationFilterAttribute
    {
        /// <summary>
        /// The HTTP client static instance. This is used to get endorsements and hence a static instance.
        /// </summary>
        public static HttpClient httpClient = new HttpClient();

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = BotAuthenticationFilter.GenerateUnauthorizedResponse(
                    actionContext.Request, 
                    "Authorization header is missing on the request");
            }

            try
            {
                var authContext = await AuthenticationHelper.GetRequestAuthenticationContextAsync(
                    actionContext.Request.Headers.Authorization.Parameter,
                    httpClient);

                AuthenticationHelper.SetRequestAuthenticationContext(authContext);
            }
            catch (UnauthorizedAccessException unauthEx)
            {
                actionContext.Response = BotAuthenticationFilter.GenerateUnauthorizedResponse(
                    actionContext.Request,
                    unauthEx.Message);
            }

            await base.OnAuthorizationAsync(actionContext, cancellationToken);
        }

        /// <summary>
        /// Generates <see cref="HttpStatusCode.Unauthorized"/> response for the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="reason">The reason phrase for unauthorized status code.</param>
        /// <returns>A response with status code unauthorized.</returns>
        private static HttpResponseMessage GenerateUnauthorizedResponse(HttpRequestMessage request, string reason = "")
        {
            string host = request.RequestUri.DnsSafeHost;
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Headers.Add("WWW-Authenticate", string.Format("Bearer realm=\"{0}\"", host));
            if (!string.IsNullOrEmpty(reason))
            {
                response.Content = new StringContent(reason, Encoding.UTF8);
            }

            return response;
        }
    }
}
