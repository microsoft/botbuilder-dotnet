// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Runtime.Plugins
{
    internal class AdapterServiceFilter
    {
        private readonly ResourcesSettings _resourcesSettings;
        private readonly IServiceCollection _services;

        public AdapterServiceFilter(IServiceCollection services, ResourcesSettings resourcesSettings)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _resourcesSettings = resourcesSettings ?? throw new ArgumentNullException(nameof(resourcesSettings));
        }

        public bool OnAddServiceDescriptor(ServiceDescriptor sd)
        {
            // By default, don't allow plugins to register IBotFrameworkHttpAdapters. 
            // If we don't know the adapter type, we cannot confirm that it was chosen by the user and that it was enabled.
            if (sd.ServiceType.Equals(typeof(IBotFrameworkHttpAdapter)))
            {
                return false;
            }

            // Allowed adapters must be registered on their actual type and implement IBotFrameworkHttpAdapter
            if (typeof(IBotFrameworkHttpAdapter).IsAssignableFrom(sd.ServiceType))
            {
                // Allowed adapters must be configured in the runtime adapter settings and enabled
                var adapterSettings = _resourcesSettings.Adapters.SingleOrDefault(a => a.Name == sd.ServiceType.FullName && a.Enabled);

                if (adapterSettings != null)
                {
                    // The adapter is allowed: Add the corresponding registrations for the types we need in our system.
                    // First, check if it is of type BotAdapter so it participates in common BotAdapter things (such as telemetry middleware).
                    bool isBotAdapter = false;

                    if (typeof(BotAdapter).IsAssignableFrom(sd.ServiceType))
                    {
                        isBotAdapter = true;
                    }

                    // Depending on the registration type on the plugin code, register the instance or factory under IBotFrameworkHttpAdapter and BotAdapter. 
                    if (sd.ImplementationFactory != null)
                    {
                        // If plugin author registered adapter through a factory, i.e. services.AddSingleton<ContosoAdapter>(sp => new ContosoAdapter(config));
                        _services.Add(ServiceDescriptor.Singleton<IBotFrameworkHttpAdapter>(sd.ImplementationFactory as Func<IServiceProvider, IBotFrameworkHttpAdapter>));
                        if (isBotAdapter)
                        {
                            _services.Add(ServiceDescriptor.Singleton<BotAdapter>(sd.ImplementationFactory as Func<IServiceProvider, BotAdapter>));
                        }
                    }
                    else
                    {
                        // If plugin author registered adapter through an instance, i.e. services.AddSingleton<ContosoAdapter>(new ContosoAdapter(config)); 
                        _services.Add(ServiceDescriptor.Singleton<IBotFrameworkHttpAdapter>(sd.ImplementationInstance as IBotFrameworkHttpAdapter));
                        if (isBotAdapter)
                        {
                            _services.Add(ServiceDescriptor.Singleton<BotAdapter>(sd.ImplementationInstance as BotAdapter));
                        }
                    }

                    _services.AddSingleton(adapterSettings);
                    return true;
                }
            }

            return false;
        }
    }
}
