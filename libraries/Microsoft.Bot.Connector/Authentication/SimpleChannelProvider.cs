// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    public class SimpleChannelProvider : IChannelProvider
    {
        public string Issuer { get; set; }

        public string OpenIdMetadataUrl { get; set; }

        public SimpleChannelProvider()
        {
        }

        public SimpleChannelProvider(string issuer, string openIdMetadataUrl)
        {
            this.Issuer = issuer;
            this.OpenIdMetadataUrl = openIdMetadataUrl;
        }

        /// <summary>
        /// Determines if a given Auth header is from the configured Bot Framework Service Channel
        /// </summary>
        /// <param name="authHeader">Bearer Token, in the "Bearer [Long String]" Format.</param>
        /// <returns>True, if the token was issued by the Channel. Otherwise, false.</returns>
        public Task<bool> IsTokenFromChannel(string authHeader)
        {
            // The Auth Header generally looks like this:
            // "Bearer eyJ0e[...Big Long String...]XAiO"
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                // No token. Can't be an emulator token. 
                return Task.FromResult(false);
            }

            string[] parts = authHeader?.Split(' ');
            if (parts.Length != 2)
            {
                // Emulator tokens MUST have exactly 2 parts. If we don't have 2 parts, it's not an emulator token
                return Task.FromResult(false);
            }

            string authScheme = parts[0];
            string bearerToken = parts[1];

            // We now have an array that should be:
            // [0] = "Bearer"
            // [1] = "[Big Long String]"
            if (authScheme != "Bearer")
            {
                // The scheme from the emulator MUST be "Bearer"
                return Task.FromResult(false);
            }

            // Parse the Big Long String into an actual token. 
            JwtSecurityToken token = new JwtSecurityToken(bearerToken);

            // Is there an Issuer? 
            if (string.IsNullOrWhiteSpace(token.Issuer))
            {
                // No Issuer, means it's not from the Emulator. 
                return Task.FromResult(false);
            }

            // Is the token issues by a source that was configured?
            return Task.FromResult(this.Issuer == token.Issuer);
        }

        public Task<string> GetIssuerAsync()
        {
            return Task.FromResult(this.Issuer);
        }

        public Task<string> GetOpenIdMetadataUrlAsync()
        {
            return Task.FromResult(this.OpenIdMetadataUrl);
        }
    }
}
