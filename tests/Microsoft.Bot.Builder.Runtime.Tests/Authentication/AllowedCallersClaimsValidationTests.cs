// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Runtime.Authentication;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Authentication
{
    public class AllowedCallersClaimsValidationTests
    {
        private string version = "1.0";

        private string audienceClaim = Guid.NewGuid().ToString();

        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            string primaryAppId = Guid.NewGuid().ToString();
            string secondaryAppId = Guid.NewGuid().ToString();

            // Null allowed callers
            yield return new object[]
            {
                (string)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            // Empty allowed callers array
            yield return new object[]
            {
                (string)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { AllowedCallersClaimsValidator.DefaultAllowedCallersKey, JToken.FromObject(new string[0]) }
                })
            };

            // Allow any caller
            yield return new object[]
            {
                primaryAppId,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { AllowedCallersClaimsValidator.DefaultAllowedCallersKey, JToken.FromObject(new string[] { "*" }) }
                })
            };

            // Specify allowed caller
            yield return new object[]
            {
                primaryAppId,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { AllowedCallersClaimsValidator.DefaultAllowedCallersKey, JToken.FromObject(new string[] { $"{primaryAppId}" }) }
                })
            };

            // Specify multiple callers
            yield return new object[]
            {
                primaryAppId,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { AllowedCallersClaimsValidator.DefaultAllowedCallersKey, JToken.FromObject(new string[] { $"{primaryAppId}", $"{secondaryAppId}" }) }
                })
            };

            // Blocked caller throws exception
            yield return new object[]
            {
                primaryAppId,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { AllowedCallersClaimsValidator.DefaultAllowedCallersKey, JToken.FromObject(new string[] { $"{secondaryAppId}" }) }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]

        public async Task AcceptAllowedCallersArray(string allowedCallerClaimId, IConfiguration configuration)
        {
            var validator = new AllowedCallersClaimsValidator(configuration);

            var allowedCallersList = configuration.GetSection(AllowedCallersClaimsValidator.DefaultAllowedCallersKey).Get<string[]>();

            if (allowedCallersList != null)
            {
                var claims = CreateCallerClaims(allowedCallerClaimId);

                if (allowedCallersList.Contains(allowedCallerClaimId) || allowedCallersList.Contains("*"))
                {
                    await validator.ValidateClaimsAsync(claims);
                }
                else
                {
                    Exception ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => validator.ValidateClaimsAsync(claims));

                    Assert.Equal(
                        $"Received a request from a bot with an app ID of \"{allowedCallerClaimId}\". To enable requests from this caller," +
                        $" add the app ID to your ${AllowedCallersClaimsValidator.DefaultAllowedCallersKey} configuration.", ex.Message);
                }
            }
        }

        private List<Claim> CreateCallerClaims(string appId)
        {
            return new List<Claim>()
            {
                new Claim(AuthenticationConstants.AppIdClaim, appId),
                new Claim(AuthenticationConstants.VersionClaim, version),
                new Claim(AuthenticationConstants.AudienceClaim, audienceClaim),
            };
        }
    }
}
