// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Runtime.Providers.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Providers.Storage
{
    public class MemoryStorageProviderTests
    {
        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            yield return new object[]
            {
                (JObject)null
            };

            yield return new object[]
            {
                new JObject()
            };

            yield return new object[]
            {
                new JObject
                {
                    {
                        "shallowObject", new JObject
                        {
                            { "foo", "bar" }
                        }
                    },
                    {
                        "deepObject", new JObject
                        {
                            {
                                "nested", new JObject
                                {
                                    { "foo", "bar" }
                                }
                            },
                            {
                                "foo", "bar"
                            }
                        }
                    },
                    {
                        "emptyObject", new JObject()
                    },
                    {
                        "stringProperty", "stringValue"
                    },
                    {
                        "numberProperty", 1
                    },
                    {
                        "booleanProperty", true
                    },
                    {
                        "nullProperty", null
                    },
                    {
                        "arrayProperty", new JArray(1, 2, 3)
                    }
                }
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]
        public void ConfigureServices_Succeeds(JObject content)
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            new MemoryStorageProvider
            {
                Content = content
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<IStorage, MemoryStorage>(
                services,
                provider,
                ServiceLifetime.Singleton);
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
                () => new MemoryStorageProvider().ConfigureServices(services, configuration));
        }
    }
}
