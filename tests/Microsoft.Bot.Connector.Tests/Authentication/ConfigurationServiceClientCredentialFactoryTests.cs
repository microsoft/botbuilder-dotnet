// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class ConfigurationServiceClientCredentialFactoryTests
    {
        private const string TestAppId = "foo";
        private const string TestAppPassword = "bar";
        private const string TestAppTenantId = "test";

        [Fact]
        public void CanCreateMultiTenantAppWithoutCredentials()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource())
            });

            _ = new ConfigurationServiceClientCredentialFactory(config);
        }

        [Fact]
        public void CanCreateMultiTenantAppWithEmptyCredentials()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, string.Empty },
                        { MicrosoftAppCredentials.MicrosoftAppPasswordKey, string.Empty }
                    }
                })
            });

            _ = new ConfigurationServiceClientCredentialFactory(config);
        }

        [Fact]
        public void CanCreateMultiTenantAppWithCredentials()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, TestAppId },
                        { MicrosoftAppCredentials.MicrosoftAppPasswordKey, TestAppPassword }
                    }
                })
            });

            _ = new ConfigurationServiceClientCredentialFactory(config);
        }

        [Fact]
        public void CanCreateMultiTenantAppWithAppTypeAndTenantId()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "MultiTenant" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, TestAppId },
                        { MicrosoftAppCredentials.MicrosoftAppPasswordKey, TestAppPassword },
                        { MicrosoftAppCredentials.MicrosoftAppTenantIdKey, TestAppTenantId }
                    }
                })
            });

            _ = new ConfigurationServiceClientCredentialFactory(config);
        }

        [Fact]
        public void CanCreateSingleTenantApp()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "SingleTenant" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, TestAppId },
                        { MicrosoftAppCredentials.MicrosoftAppPasswordKey, TestAppPassword },
                        { MicrosoftAppCredentials.MicrosoftAppTenantIdKey, TestAppTenantId }
                    }
                })
            });

            _ = new ConfigurationServiceClientCredentialFactory(config);
        }

        [Fact]
        public void CannotCreateSingleTenantAppWithoutTenantId()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "SingleTenant" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, TestAppId },
                        { MicrosoftAppCredentials.MicrosoftAppPasswordKey, TestAppPassword }
                    }
                })
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new ConfigurationServiceClientCredentialFactory(config);
            });
        }

        [Fact]
        public void CannotCreateSingleTenantAppWithoutAppId()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "SingleTenant" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, string.Empty },
                        { MicrosoftAppCredentials.MicrosoftAppPasswordKey, TestAppPassword },
                        { MicrosoftAppCredentials.MicrosoftAppTenantIdKey, TestAppTenantId }
                    }
                })
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new ConfigurationServiceClientCredentialFactory(config);
            });
        }

        [Fact]
        public void CannotCreateSingleTenantAppWithoutPassword()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "SingleTenant" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, TestAppId },
                        { MicrosoftAppCredentials.MicrosoftAppPasswordKey, string.Empty },
                        { MicrosoftAppCredentials.MicrosoftAppTenantIdKey, TestAppTenantId }
                    }
                })
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new ConfigurationServiceClientCredentialFactory(config);
            });
        }

        [Fact]
        public void CanCreateManagedIdentityApp()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "UserAssignedMSI" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, TestAppId },
                        { MicrosoftAppCredentials.MicrosoftAppTenantIdKey, TestAppTenantId }
                    }
                })
            });

            _ = new ConfigurationServiceClientCredentialFactory(config);
        }

        [Fact]
        public void CannotCreateManagedIdentityAppWithoutTenantId()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "UserAssignedMSI" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, TestAppId }
                    }
                })
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new ConfigurationServiceClientCredentialFactory(config);
            });
        }

        [Fact]
        public void CannotCreateManagedIdentityAppWithoutAppId()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "UserAssignedMSI" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, string.Empty },
                        { MicrosoftAppCredentials.MicrosoftAppTenantIdKey, TestAppTenantId }
                    }
                })
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new ConfigurationServiceClientCredentialFactory(config);
            });
        }

        [Fact]
        public void CannotCreateManagedIdentityAppWithPassword()
        {
            var config = new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        { MicrosoftAppCredentials.MicrosoftAppTypeKey, "UserAssignedMSI" },
                        { MicrosoftAppCredentials.MicrosoftAppIdKey, TestAppId },
                        { MicrosoftAppCredentials.MicrosoftAppPasswordKey, TestAppPassword },
                        { MicrosoftAppCredentials.MicrosoftAppTenantIdKey, TestAppTenantId }
                    }
                })
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new ConfigurationServiceClientCredentialFactory(config);
            });
        }
    }
}
