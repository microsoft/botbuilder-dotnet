// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Runtime.Tests.Providers
{
    public class ProviderTestDataGenerator
    {
        public static IEnumerable<object[]> GetConfigureServicesArgumentNullExceptionData()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot(new JObject());

            yield return new object[] { "services", (IServiceCollection)null, (IConfiguration)null };
            yield return new object[] { "services", (IServiceCollection)null, configuration };
            yield return new object[] { "configuration", services, (IConfiguration)null };
        }
    }
}
