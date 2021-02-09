// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Integration.Runtime.Extensions;
using Microsoft.Bot.Builder.Integration.Runtime.Settings;
using Microsoft.Bot.Builder.Integration.Runtime.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class SkillRegistrationTests
    {
        public static IEnumerable<object[]> SkillRegistrationTestData()
        {
            var guid = Guid.NewGuid().ToString();

            // params: object settings, string appId, Type exceptionType
            yield return new object[]
            {
                null,
                guid,
                typeof(UnauthorizedAccessException)
            };
            yield return new object[]
            {
                new SkillSettings() { },
                Guid.NewGuid().ToString(),
                typeof(UnauthorizedAccessException)
            };
            yield return new object[]
            {
                new SkillSettings() { AllowedCallers = new[] { "*" } },
                Guid.NewGuid().ToString(),
                null
            };
            yield return new object[]
            {
                new SkillSettings() { AllowedCallers = new[] { guid } },
                guid,
                null
            };
            yield return new object[]
            {
                new SkillSettings() { AllowedCallers = new[] { guid } },
                Guid.NewGuid().ToString(),
                typeof(UnauthorizedAccessException)
            };
        }

        [Theory]
        [MemberData(nameof(SkillRegistrationTestData))]
        public async Task AddBotRuntimeSkills(object settings, string appId, Type exceptionType)
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            var skillSettings = settings as SkillSettings;

            services.AddSingleton<IStorage, MemoryStorage>();
            services.AddSingleton<ICredentialProvider, SimpleCredentialProvider>();
            services.AddSingleton<BotAdapter, BotFrameworkAdapter>();
            services.AddSingleton<IBot, ActivityHandler>();

            // Test
            services.AddBotRuntimeSkills(skillSettings);

            // Assert
            var provider = services.BuildServiceProvider();

            Assertions.AssertService<SkillConversationIdFactoryBase, SkillConversationIdFactory>(services, provider, ServiceLifetime.Singleton);
            Assertions.AssertService<BotFrameworkClient, SkillHttpClient>(services, provider, ServiceLifetime.Transient);
            Assertions.AssertService<ChannelServiceHandler, SkillHandler>(services, provider, ServiceLifetime.Singleton);
            Assertions.AssertService<AuthenticationConfiguration>(
                services,
                provider,
                ServiceLifetime.Singleton,
                async authConfig =>
                {
                    var versionClaim = new Claim(AuthenticationConstants.VersionClaim, "1.0");
                    var appIdClaim = new Claim(AuthenticationConstants.AppIdClaim, appId);

                    if (exceptionType == null)
                    {
                        await authConfig.ClaimsValidator.ValidateClaimsAsync(new Claim[] { versionClaim, appIdClaim });
                    }
                    else
                    {
                        await Assert.ThrowsAsync(exceptionType, () => authConfig.ClaimsValidator.ValidateClaimsAsync(new Claim[] { versionClaim, appIdClaim }));
                    }
                });
            
            await Task.CompletedTask;
        }
    }
}
