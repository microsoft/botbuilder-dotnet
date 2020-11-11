// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Providers.Credentials;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using ICredentials = Microsoft.Bot.Connector.Authentication.ICredentialProvider;

namespace Microsoft.Bot.Builder.Runtime.Tests.Providers.Credentials
{
    public class DeclarativeCredentialsProviderTests
    {
        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            string applicationId = Guid.NewGuid().ToString();
            string applicationPassword = Guid.NewGuid().ToString();

            yield return new object[]
            {
                (StringExpression)null,
                (StringExpression)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(),
                (Action<SimpleCredentialProvider>)((credentialProvider) =>
                {
                    Assert.Null(credentialProvider.AppId);
                    Assert.Null(credentialProvider.Password);
                })
            };

            yield return new object[]
            {
                new StringExpression(applicationId),
                new StringExpression(applicationPassword),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(),
                (Action<SimpleCredentialProvider>)((credentialProvider) =>
                {
                    Assert.Equal(expected: applicationId, actual: credentialProvider.AppId);
                    Assert.Equal(expected: applicationPassword, actual: credentialProvider.Password);
                })
            };

            yield return new object[]
            {
                new StringExpression("=applicationId"),
                new StringExpression("=applicationPassword"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "applicationId", applicationId },
                    { "applicationPassword", applicationPassword }
                }),
                (Action<SimpleCredentialProvider>)((credentialProvider) =>
                {
                    Assert.Equal(expected: applicationId, actual: credentialProvider.AppId);
                    Assert.Equal(expected: applicationPassword, actual: credentialProvider.Password);
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]
        public void ConfigureServices_Succeeds(
            StringExpression applicationId,
            StringExpression applicationPassword,
            IConfiguration configuration,
            Action<SimpleCredentialProvider> assertCredentialsProvider)
        {
            var services = new ServiceCollection();

            new DeclarativeCredentialsProvider
            {
                ApplicationId = applicationId,
                ApplicationPassword = applicationPassword
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<ICredentials, SimpleCredentialProvider>(
                services,
                provider,
                ServiceLifetime.Singleton,
                assertCredentialsProvider);
        }

        [Theory]
        [MemberData(
            nameof(ProviderTestDataGenerator.GetConfigureServicesArgumentNullExceptionData),
            MemberType = typeof(ProviderTestDataGenerator))]
        public void ConfigureServices_Throws_ArgumentNullException(
            string paramName,
            IServiceCollection services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new DeclarativeCredentialsProvider().ConfigureServices(services, configuration));
        }
    }
}
