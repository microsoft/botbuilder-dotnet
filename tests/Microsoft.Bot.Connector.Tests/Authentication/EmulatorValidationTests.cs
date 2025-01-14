// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class EmulatorValidationTests
    {
        [Fact]
        public void IsTokenFromEmulator_ShouldReturnFalseOnTokenBadFormat()
        {
            //Token with bad format is not processed
            var authHeader = string.Empty;
            Assert.False(EmulatorValidation.IsTokenFromEmulator(authHeader));

            //If the token doesn't contain an issuer value it returns false
            authHeader = GenerateMockBearerToken(null);
            Assert.False(EmulatorValidation.IsTokenFromEmulator(authHeader));
        }

        [Fact]
        public void IsTokenFromEmulator_ShouldReturnFalseOnInvalidTokenIssuer()
        {
            var authHeader = GenerateMockBearerToken("https://mockIssuer.com");
            Assert.False(EmulatorValidation.IsTokenFromEmulator(authHeader));
        }

        [Fact]
        public void IsTokenFromEmulator_ShouldReturnTrueOnValidTokenIssuer()
        {
            //Validate issuer with V1 Token
            var authHeader = GenerateMockBearerToken(AuthenticationConstants.ValidTokenIssuerUrlTemplateV1);
            Assert.True(EmulatorValidation.IsTokenFromEmulator(authHeader));

            //Validate issuer with V2 Token
            authHeader = GenerateMockBearerToken(AuthenticationConstants.ValidTokenIssuerUrlTemplateV2);
            Assert.True(EmulatorValidation.IsTokenFromEmulator(authHeader));

            //Validate Government issuer with V1 Token
            authHeader = GenerateMockBearerToken(AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV1);
            Assert.True(EmulatorValidation.IsTokenFromEmulator(authHeader));

            //Validate Government issuer with V2 Token
            authHeader = GenerateMockBearerToken(AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV2);
            Assert.True(EmulatorValidation.IsTokenFromEmulator(authHeader));
        }

        private string GenerateMockBearerToken(string issuer)
        {
            var key = Encoding.UTF8.GetBytes("ThisIsASuperMockSecretKey123456789");
            var signingKey = new SymmetricSecurityKey(key);
            var tenantId = Guid.NewGuid().ToString();

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer?.Replace("{0}", tenantId),
                Audience = "https://api.example.com", 
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, "John Doe"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("tid", tenantId)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
            };

            // Create and return the JWT
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return $"Bearer {tokenHandler.WriteToken(token)}";
        }
    }
}
