// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class AllowedCallersClaimsValidationTests
    {
        private const string _version = "1.0";

        private readonly string audienceClaim = Guid.NewGuid().ToString();

        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            string primaryAppId = Guid.NewGuid().ToString();
            string secondaryAppId = Guid.NewGuid().ToString();

            // Null allowed callers
            yield return new object[]
            {
                null,
                null
            };

            // Null configuration with attempted caller
            yield return new object[]
            {
                primaryAppId,
                null
            };

            // Empty allowed callers array
            yield return new object[]
            {
                (string)null,
                new string[0]
            };

            // Allow any caller
            yield return new object[]
            {
                primaryAppId,
                new string[] { "*" }
            };

            // Specify allowed caller
            yield return new object[]
            {
                primaryAppId,
                new string[] { $"{primaryAppId}" }
            };

            // Specify multiple callers
            yield return new object[]
            {
                primaryAppId,
                new string[] { $"{primaryAppId}", $"{secondaryAppId}" }
            };

            // Blocked caller throws exception
            yield return new object[]
            {
                primaryAppId,
                new string[] { $"{secondaryAppId}" }
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]

        public async Task AcceptAllowedCallersArray(string allowedCallerClaimId, IList<string> allowList)
        {
            var validator = new AllowedCallersClaimsValidator(allowList);

            if (allowedCallerClaimId != null)
            {
                var claims = CreateCallerClaims(allowedCallerClaimId);

                if (allowList != null)
                {
                    if (allowList.Contains(allowedCallerClaimId) || allowList.Contains("*"))
                    {
                        await validator.ValidateClaimsAsync(claims);
                    }
                    else
                    {
                        await ValidateUnauthorizedAccessException(allowedCallerClaimId, validator, claims);
                    }
                }
                else
                {
                    await ValidateUnauthorizedAccessException(allowedCallerClaimId, validator, claims);
                }
            }
        }

        private static async Task ValidateUnauthorizedAccessException(string allowedCallerClaimId, AllowedCallersClaimsValidator validator, List<Claim> claims)
        {
            Exception ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => validator.ValidateClaimsAsync(claims));

            Assert.Contains(allowedCallerClaimId, ex.Message);
        }

        private List<Claim> CreateCallerClaims(string appId)
        {
            return new List<Claim>()
            {
                new Claim(AuthenticationConstants.AppIdClaim, appId),
                new Claim(AuthenticationConstants.VersionClaim, _version),
                new Claim(AuthenticationConstants.AudienceClaim, audienceClaim),
            };
        }
    }
}
