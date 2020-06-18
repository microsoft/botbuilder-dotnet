// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class JwtTokenExtractorTests
    {
        private const int NTEBADKEYSET = unchecked((int)0x80090016);

        private static readonly HttpClient Client = new HttpClient();

        public JwtTokenExtractorTests()
        {
            // Disable TokenLifetime validation
            EmulatorValidation.ToBotFromEmulatorTokenValidationParameters.ValidateLifetime = false;
            ChannelValidation.ToBotFromChannelTokenValidationParameters.ValidateLifetime = false;
            GovernmentChannelValidation.ToBotFromGovernmentChannelTokenValidationParameters.ValidateLifetime = false;
        }

        [Fact]
        public void JwtTokenExtractor_GovernmentValidationParameters_ShouldValidateSigningKey()
        {
            Assert.True(GovernmentChannelValidation.ToBotFromGovernmentChannelTokenValidationParameters.ValidateIssuerSigningKey);
        }

        [Fact]
        public void JwtTokenExtractor_ChannelValidationParameters_ShouldValidateSigningKey()
        {
            Assert.True(ChannelValidation.ToBotFromChannelTokenValidationParameters.ValidateIssuerSigningKey);
        }

        [Fact]
        public void JwtTokenExtractor_EnterpriseChannelValidationParameters_ShouldValidateSigningKey()
        {
            Assert.True(EnterpriseChannelValidation.ToBotFromEnterpriseChannelTokenValidationParameters.ValidateIssuerSigningKey);
        }

        [Fact]
        [Trait("TestCategory", "WindowsOnly")]
        public async Task JwtTokenExtractor_WithExpiredCert_ShouldNotAllowCertSigningKey()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var now = DateTimeOffset.UtcNow;
                var cn = "test.cert.botframework.com";

                // Create expired self-signed certificate
                var cert = CreateSelfSignedCertificate("test.cert.botframework.com", from: now.AddDays(-10), to: now.AddDays(-9));

                // Build token extractor and use it to validate a token created from the cert
                // Since the cert is expired, it should fail since the signing key is bad
                await Assert.ThrowsAnyAsync<SecurityTokenInvalidSigningKeyException>(() => BuildExtractorAndValidateToken(cert));

                DeleteKeyContainer(cn);
            }
        }

        [Fact]
        [Trait("TestCategory", "WindowsOnly")]
        public async Task JwtTokenExtractor_WithValidCert_ShouldNotAllowCertSigningKey()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var now = DateTimeOffset.UtcNow;
                var cn = "test.cert.botframework.com";

                // Create valid self-signed certificate
                var cert = CreateSelfSignedCertificate(cn, from: now.AddDays(-10), to: now.AddDays(9));

                // Build token extractor and use it to validate a token created from the cert
                await BuildExtractorAndValidateToken(cert);

                DeleteKeyContainer(cn);
            }
        }

        private static Task<ClaimsIdentity> BuildExtractorAndValidateToken(X509Certificate2 cert, TokenValidationParameters validationParameters = null)
        {
            // Custom validation parameters that allow us to test the extractor logic
            var tokenValidationParams = validationParameters ?? CreateTokenValidationParameters(cert);

            // Build Jwt extractor to be tested
            var tokenExtractor = new JwtTokenExtractor(
                Client,
                tokenValidationParams,
                "https://login.botframework.com/v1/.well-known/openidconfiguration",
                AuthenticationConstants.AllowedSigningAlgorithms);

            var token = CreateTokenForCertificate(cert);

            return tokenExtractor.GetIdentityAsync($"Bearer {token}", "test");
        }

        private static string CreateTokenForCertificate(X509Certificate2 cert)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new X509SecurityKey(cert), SecurityAlgorithms.RsaSha256Signature)
            };

            JwtSecurityToken stoken = (JwtSecurityToken)tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(stoken);
        }

        private static TokenValidationParameters CreateTokenValidationParameters(X509Certificate2 cert)
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidIssuers = new[] { AuthenticationConstants.ToBotFromChannelTokenIssuer },

                // Audience validation takes place in JwtTokenExtractor
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
                IssuerSigningKey = new X509SecurityKey(cert),
                IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) => new List<X509SecurityKey> { new X509SecurityKey(cert) },
            };
        }

        private static X509Certificate2 CreateSelfSignedCertificate(string cn, DateTimeOffset from, DateTimeOffset to)
        {
            var parameters = new CspParameters(24, "Microsoft Enhanced RSA and AES Cryptographic Provider")
            {
                KeyContainerName = cn,
                Flags = CspProviderFlags.NoPrompt,
            };

            X509Certificate2 cert;

            const int KeySize = 4096;
            using (var provider = new RSACryptoServiceProvider(KeySize, parameters))
            {
                var request = new CertificateRequest(
                        $"CN={cn}",
                        provider,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);

                cert = request.CreateSelfSigned(from, to);
            }

            return cert;
        }

        private static void DeleteKeyContainer(string cn)
        {
            // %APPDATA%\Microsoft\Crypto\RSA
            var parameters = new CspParameters()
            {
                KeyContainerName = cn,
                Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseExistingKey,
            };

            try
            {
                using (var provider = new RSACryptoServiceProvider(parameters))
                {
                    provider.PersistKeyInCsp = false;
                    provider.Clear();
                }
            }
            catch (CryptographicException error) when (IsKeySetDoesNotExist(error))
            {
                Debug.WriteLine(error.ToString());
            }
        }

        private static bool IsKeySetDoesNotExist(Exception error) =>
            error.HResult == NTEBADKEYSET;
    }
}
