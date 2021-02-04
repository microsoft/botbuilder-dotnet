// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Runtime.Tests
{
    public class ServiceDescriptorSearchOptions
    {
        public ServiceDescriptorSearchOptions(SearchFunction search)
        {
            this.Search = search ?? throw new ArgumentNullException(nameof(search));
        }

        public delegate IEnumerable<ServiceDescriptor> SearchFunction(IEnumerable<ServiceDescriptor> descriptors);

        public SearchFunction Search { get; }

        public static ServiceDescriptorSearchOptions SearchByImplementationType<TImplementation>()
        {
            return SearchByImplementationType(typeof(TImplementation));
        }

        public static ServiceDescriptorSearchOptions SearchByImplementationType(Type implementationType)
        {
            return new ServiceDescriptorSearchOptions((descriptors) =>
                descriptors.Where(d => d.ImplementationType == implementationType));
        }

        public static ServiceDescriptorSearchOptions SearchByServiceType<TService>()
        {
            return SearchByServiceType(typeof(TService));
        }

        public static ServiceDescriptorSearchOptions SearchByServiceType(Type implementationType)
        {
            return new ServiceDescriptorSearchOptions((descriptors) =>
                descriptors.Where(d => d.ServiceType == implementationType));
        }
    }
}
