// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    public class ResourceExplorerOptionsTests
    {
        public static IEnumerable<object[]> ResourceExplorerOptionsTestData()
        {
            var testProviders = new ResourceProvider[] { new TestResourceProvider() };
            var testDeclarativeTypes = new DeclarativeType[] { new DeclarativeType<ResourceExplorerOptionsTests>("test") };
            var testConverterFactories = new JsonConverterFactory[] { new JsonConverterFactory<ActivityConverter>() };

            // params: 
            //      IEnumerable<ResourceProvider> resourceProviders
            //      IEnumerable<DeclarativeType> declarativeTypes
            //      IEnumerable<JsonConverterFactory> converterFactories
            yield return new object[] { null, null, null };
            yield return new object[] { testProviders, null, null };
            yield return new object[] { null, testDeclarativeTypes, null };
            yield return new object[] { null, null, testConverterFactories };
            yield return new object[] { testProviders, testDeclarativeTypes, testConverterFactories };
        }

        [Theory]
        [MemberData(nameof(ResourceExplorerOptionsTestData))]
        public void TestResourceExplorerOptions(
            IEnumerable<ResourceProvider> resourceProviders, 
            IEnumerable<DeclarativeType> declarativeTypes, 
            IEnumerable<JsonConverterFactory> converterFactories)
        {
            // Test
            var options = new ResourceExplorerOptions(resourceProviders, declarativeTypes, converterFactories);

            // Assert
            Assert.Equal(resourceProviders, options.Providers);
            Assert.Equal(declarativeTypes, options.TypeRegistrations);
            Assert.Equal(converterFactories, options.ConverterFactories);
        }

        internal class TestResourceProvider : ResourceProvider
        {
            public TestResourceProvider()
                : base(null)
            {
            }

            public override IEnumerable<Resource> GetResources(string extension)
            {
                throw new NotImplementedException();
            }

            public override void Refresh()
            {
                throw new NotImplementedException();
            }

            public override bool TryGetResource(string id, out Resource resource)
            {
                throw new NotImplementedException();
            }
        }
    }
}
