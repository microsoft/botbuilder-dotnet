// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class ClaimsExtensionsTests
    {
        [Fact]
        public void IsSkillClaimTest()
        {
            var claims = new List<Claim>();
            var audience = Guid.NewGuid().ToString();
            var appId = Guid.NewGuid().ToString();

            // Empty list of claims
            Assert.False(claims.IsSkillClaim());

            // No Audience claim
            claims.Add(new Claim(AuthenticationConstants.VersionClaim, "1.0"));
            Assert.False(claims.IsSkillClaim());

            // Emulator Audience claim
            claims.Add(new Claim(AuthenticationConstants.AudienceClaim, AuthenticationConstants.ToBotFromChannelTokenIssuer));
            Assert.False(claims.IsSkillClaim());

            // No AppId claim
            claims.RemoveAt(claims.Count - 1);
            claims.Add(new Claim(AuthenticationConstants.AudienceClaim, audience));
            Assert.False(claims.IsSkillClaim());

            // AppId != Audience
            claims.Add(new Claim(AuthenticationConstants.AppIdClaim, audience));
            Assert.False(claims.IsSkillClaim());

            // Anonymous skill app id
            claims.RemoveAt(claims.Count - 1);
            claims.Add(new Claim(AuthenticationConstants.AppIdClaim, AuthenticationConstants.AnonymousSkillAppId));
            Assert.True(claims.IsSkillClaim());

            // All checks pass, should be good now
            claims.RemoveAt(claims.Count - 1);
            claims.Add(new Claim(AuthenticationConstants.AppIdClaim, appId));
            Assert.True(claims.IsSkillClaim());
        }

        [Fact]
        public void GetAppIdFromClaimsTests()
        {
            var v1Claims = new List<Claim>();
            var v2Claims = new List<Claim>();
            var appId = Guid.NewGuid().ToString();

            // Empty list
            Assert.Null(v1Claims.GetAppIdFromClaims());

            // AppId there but no version (assumes v1)
            v1Claims.Add(new Claim(AuthenticationConstants.AppIdClaim, appId));
            Assert.Equal(appId, v1Claims.GetAppIdFromClaims());

            // AppId there with v1 version
            v1Claims.Add(new Claim(AuthenticationConstants.VersionClaim, "1.0"));
            Assert.Equal(appId, v1Claims.GetAppIdFromClaims());

            // v2 version but no azp
            v2Claims.Add(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
            Assert.Null(v2Claims.GetAppIdFromClaims());

            // v2 version with azp
            v2Claims.Add(new Claim(AuthenticationConstants.AuthorizedParty, appId));
            Assert.Equal(appId, v2Claims.GetAppIdFromClaims());
        }
    }
}
