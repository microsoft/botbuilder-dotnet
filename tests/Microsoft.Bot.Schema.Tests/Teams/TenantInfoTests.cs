// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TenantInfoTests
    {
        [Fact]
        public void TenantInfoInits()
        {
            var id = "123456-7890-abcd-efgh-ijklmno";
            
            var tenantInfo = new TenantInfo(id);

            Assert.NotNull(tenantInfo);
            Assert.IsType<TenantInfo>(tenantInfo);
            Assert.Equal(id, tenantInfo.Id);
        }
        
        [Fact]
        public void TenantInfoInitsWithNoArgs()
        {
            var tenantInfo = new TenantInfo();

            Assert.NotNull(tenantInfo);
            Assert.IsType<TenantInfo>(tenantInfo);
        }
    }
}
