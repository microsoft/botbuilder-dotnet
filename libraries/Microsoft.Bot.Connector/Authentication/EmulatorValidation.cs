// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Validates and Examines JWT tokens from the Bot Framework Emulator.
    /// </summary>
    public static class EmulatorValidation
    {
        /// <summary>
        /// TO BOT FROM EMULATOR: Token validation parameters when connecting to a channel.
        /// </summary>
        public static readonly TokenValidationParameters ToBotFromEmulatorTokenValidationParameters =
            new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/",                    // Auth v3.1, 1.0 token
                    "https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0",      // Auth v3.1, 2.0 token
                    "https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/",                    // Auth v3.2, 1.0 token
                    "https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0",      // Auth v3.2, 2.0 token
                    "https://sts.windows.net/cab8a31a-1906-4287-a0d8-4eef66b95f6e/",                    // Auth for US Gov, 1.0 token
                    "https://login.microsoftonline.us/cab8a31a-1906-4287-a0d8-4eef66b95f6e/v2.0",       // Auth for US Gov, 2.0 token
                    "https://login.microsoftonline.us/f8cdef31-a31e-4b4a-93e4-5f571e91255a/",           // Auth for US Gov, 1.0 token
                    "https://login.microsoftonline.us/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0",       // Auth for US Gov, 2.0 token
                },
                ValidateAudience = false,   // Audience validation takes place manually in code.
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
            };

        /// <summary>
        /// Determines if a given Auth header is from the Bot Framework Emulator.
        /// </summary>
        /// <param name="authHeader">Bearer Token, in the "Bearer [Long String]" Format.</param>
        /// <returns>True, if the token was issued by the Emulator. Otherwise, false.</returns>
        public static bool IsTokenFromEmulator(string authHeader)
        {
            if (!JwtTokenValidation.IsValidTokenFormat(authHeader))
            {
                return false;
            }

            // We know is a valid token, split it and work with it:
            // [0] = "Bearer"
            // [1] = "[Big Long String]"
            var bearerToken = authHeader.Split(' ')[1];

            // Parse the Big Long String into an actual token.
            var token = new JwtSecurityToken(bearerToken);

            // Is there an Issuer?
            if (string.IsNullOrWhiteSpace(token.Issuer))
            {
                // No Issuer, means it's not from the Emulator.
                return false;
            }

            // Is the token issues by a source we consider to be the emulator?
            if (!ToBotFromEmulatorTokenValidationParameters.ValidIssuers.Contains(token.Issuer))
            {
                // Not a Valid Issuer. This is NOT a Bot Framework Emulator Token.
                return false;
            }

            // The Token is from the Bot Framework Emulator. Success!
            return true;
        }
    }
}
