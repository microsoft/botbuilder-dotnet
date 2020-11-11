// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders
{
    public class BuilderTestDataGenerator
    {
        public static IEnumerable<object[]> GetBuildArgumentNullExceptionData()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot(new JObject());

            yield return new object[] { "services", (IServiceProvider)null, (IConfiguration)null };
            yield return new object[] { "services", (IServiceProvider)null, configuration };
            yield return new object[] { "configuration", (IServiceProvider)services.BuildServiceProvider(), (IConfiguration)null };
        }
    }
}
