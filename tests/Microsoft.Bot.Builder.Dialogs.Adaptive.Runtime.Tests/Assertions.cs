// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests
{
    public static class Assertions
    {
        public static IOptions<TOptions> AssertOptions<TOptions>(
            IServiceProvider provider,
            Action<TOptions> assert = null)
            where TOptions : class, new()
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var options = provider.GetService<IOptions<TOptions>>();

            Assert.NotNull(options);
            assert?.Invoke(options.Value);

            return options;
        }

        public static TService AssertService<TService>(
            IServiceCollection services,
            IServiceProvider provider,
            ServiceLifetime lifetime,
            Action<TService> assert = null,
            ServiceDescriptorSearchOptions searchOptions = null)
        {
            return AssertService<TService, TService>(
                services,
                provider,
                lifetime,
                assert,
                searchOptions);
        }

        public static TImplementation AssertService<TService, TImplementation>(
            IServiceCollection services,
            IServiceProvider provider,
            ServiceLifetime lifetime,
            Action<TImplementation> assert = null,
            ServiceDescriptorSearchOptions searchOptions = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            ServiceDescriptor descriptor = AssertServiceDescriptor<TService>(
                services,
                provider,
                lifetime,
                searchOptions);

            object service = GetService(provider, descriptor);

            Assert.NotNull(service);
            Assert.IsType<TImplementation>(service);
            Assert.IsAssignableFrom(descriptor.ServiceType, service);

            var result = (TImplementation)service;
            assert?.Invoke(result);

            return result;
        }

        public static ServiceDescriptor AssertServiceDescriptor<TService>(
            IServiceCollection services,
            IServiceProvider provider,
            ServiceLifetime lifetime,
            ServiceDescriptorSearchOptions searchOptions = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            searchOptions ??= ServiceDescriptorSearchOptions.SearchByServiceType<TService>();

            IList<ServiceDescriptor> descriptors = searchOptions.Search(services).ToList();

            Assert.Equal(expected: 1, actual: descriptors.Count);

            ServiceDescriptor descriptor = descriptors[0];
            Assert.Equal(expected: lifetime, actual: descriptor.Lifetime);
            Assert.Equal(expected: typeof(TService), actual: descriptor.ServiceType);

            return descriptor;
        }

        public static TException AssertServiceThrows<TService, TException>(
            IServiceCollection services,
            IServiceProvider provider,
            ServiceLifetime lifetime,
            Action<TException> assert = null,
            ServiceDescriptorSearchOptions searchOptions = null)
            where TException : Exception
        {
            return AssertServiceThrows<TService, TService, TException>(
                services,
                provider,
                lifetime,
                assert,
                searchOptions);
        }

        public static TException AssertServiceThrows<TService, TImplementation, TException>(
            IServiceCollection services,
            IServiceProvider provider,
            ServiceLifetime lifetime,
            Action<TException> assert = null,
            ServiceDescriptorSearchOptions searchOptions = null)
            where TException : Exception
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            ServiceDescriptor descriptor = AssertServiceDescriptor<TService>(
                services,
                provider,
                lifetime,
                searchOptions);

            Assert.True(descriptor.ServiceType.IsAssignableFrom(typeof(TImplementation)));

            TException exception = Assert.Throws<TException>(() => GetService(provider, descriptor));
            assert?.Invoke(exception);

            return exception;
        }

        private static object GetService(IServiceProvider provider, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(provider);
            }

            IEnumerable<object> services = provider.GetServices(descriptor.ServiceType);

            if (descriptor.ImplementationType != null)
            {
                services = services.Where(s => descriptor.ImplementationType == s?.GetType());
            }

            return services.LastOrDefault();
        }
    }
}
