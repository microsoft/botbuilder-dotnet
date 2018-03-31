// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.AspNet.Core
{
    /// <summary>
    /// Bot Authentication middleware.
    /// </summary>
    public class BotAuthenticationMiddleware : AuthenticationHandler<BotAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotAuthenticationMiddleware"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="encoder">The encoder.</param>
        /// <param name="clock">The clock.</param>
        public BotAuthenticationMiddleware(IOptionsMonitor<BotAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            this.Events = options.CurrentValue.Events;
        }

        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring. 
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected new JwtBearerEvents Events
        {
            get { return (JwtBearerEvents)base.Events; }
            set { base.Events = value; }
        }

        /// <summary>
        /// Authenticates the request asynchronous.
        /// </summary>
        /// <returns>Authentication result.</returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token = null;
            try
            {
                // Give application opportunity to find from a different location, adjust, or reject token
                var messageReceivedContext = new MessageReceivedContext(this.Context, Scheme, this.Options);

                // event can set the token
                await this.Events.MessageReceived(messageReceivedContext);
                if (messageReceivedContext.Result != null)
                {
                    return messageReceivedContext.Result;
                }

                // If application retrieved token from somewhere else, use that.
                token = messageReceivedContext.Token;

                if (string.IsNullOrEmpty(token))
                {
                    string authorization = Request.Headers["Authorization"];

                    // If no authorization header found, nothing to process further
                    if (string.IsNullOrEmpty(authorization))
                    {
                        return AuthenticateResult.NoResult();
                    }

                    // If no token found, no further work possible
                    if (string.IsNullOrEmpty(token))
                    {
                        return AuthenticateResult.NoResult();
                    }
                }

                // If no token found, no further work possible
                // and Authentication is not disabled fail
                if (string.IsNullOrEmpty(token))
                {
                    return AuthenticateResult.Fail("No JwtToken is present");
                }

                var authContext = await AuthenticationHelper.GetRequestAuthenticationContextAsync(token, this.Options.HttpClient);
                AuthenticationHelper.SetRequestAuthenticationContext(authContext);

                Logger.TokenValidationSucceeded();

                authContext.ClaimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Bot"));
                var principal = new ClaimsPrincipal(authContext.ClaimsIdentity);
                Context.User = principal;

                var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
                {
                    Principal = principal,
                    SecurityToken = new JwtSecurityToken(token)
                };

                await Events.TokenValidated(tokenValidatedContext);
                if (tokenValidatedContext.Result != null)
                {
                    return tokenValidatedContext.Result;
                }

                if (Options.SaveToken)
                {
                    tokenValidatedContext.Properties.StoreTokens(new[]
                    {
                        new AuthenticationToken { Name = "access_token", Value = token }
                    });
                }

                tokenValidatedContext.Success();
                return tokenValidatedContext.Result;
            }
            catch (Exception ex)
            {
                Logger.ErrorProcessingMessage(ex);

                var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
                {
                    Exception = ex
                };

                await Events.AuthenticationFailed(authenticationFailedContext);
                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }

                throw;
            }
        }
    }

    internal static class LoggingExtensions
    {
        private static Action<ILogger, string, Exception> _tokenValidationFailed;
        private static Action<ILogger, Exception> _tokenValidationSucceeded;
        private static Action<ILogger, Exception> _errorProcessingMessage;

        static LoggingExtensions()
        {
            _tokenValidationFailed = LoggerMessage.Define<string>(
                eventId: 1,
                logLevel: LogLevel.Information,
                formatString: "Failed to validate the token {Token}.");
            _tokenValidationSucceeded = LoggerMessage.Define(
                eventId: 2,
                logLevel: LogLevel.Information,
                formatString: "Successfully validated the token.");
            _errorProcessingMessage = LoggerMessage.Define(
                eventId: 3,
                logLevel: LogLevel.Error,
                formatString: "Exception occurred while processing message.");
        }

        public static void TokenValidationFailed(this ILogger logger, string token, Exception ex)
        {
            _tokenValidationFailed(logger, token, ex);
        }

        public static void TokenValidationSucceeded(this ILogger logger)
        {
            _tokenValidationSucceeded(logger, null);
        }

        public static void ErrorProcessingMessage(this ILogger logger, Exception ex)
        {
            _errorProcessingMessage(logger, ex);
        }
    }
}
