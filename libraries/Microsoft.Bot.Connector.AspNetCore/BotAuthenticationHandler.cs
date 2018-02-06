// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Connector
{
    /*
    /// <summary>
    /// Bot authentication hanlder used by <see cref="BotAuthenticationMiddleware"/>.
    /// </summary>
    public class BotAuthenticationHandler : AuthenticationHandler<BotAuthenticationOptions>
    {
        public BotAuthenticationHandler(IOptionsMonitor<BotAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
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

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (await Options.CredentialProvider.IsAuthenticationDisabledAsync())
            {
                var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "Bot") }));

                var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
                {
                    Principal = principal
                };
                tokenValidatedContext.Success();
                return tokenValidatedContext.Result;
            }

            string token = null;
            try
            {
                // Give application opportunity to find from a different location, adjust, or reject token
                var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options);

                // event can set the token
                await Events.MessageReceived(messageReceivedContext);
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

                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = authorization.Substring("Bearer ".Length).Trim();
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
                    return AuthenticateResult.Fail("No JwtToken is present and BotAuthentication is enabled!");
                }
                var authenticator = new BotAuthenticator(Options.CredentialProvider, Options.OpenIdConfiguration, Options.DisableEmulatorTokens);
                var identityToken = await authenticator.TryAuthenticateAsync(Options.Challenge, token, CancellationToken.None);

                if (identityToken.Authenticated)
                {
                    Logger.TokenValidationSucceeded();

                    identityToken.Identity.AddClaim(new Claim(ClaimTypes.Role, "Bot"));
                    var principal = new ClaimsPrincipal(identityToken.Identity);
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
                else
                {
                    Logger.TokenValidationFailed(token, null);
                    return AuthenticateResult.Fail($"Failed to authenticate JwtToken {token}");
                }

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
    */

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