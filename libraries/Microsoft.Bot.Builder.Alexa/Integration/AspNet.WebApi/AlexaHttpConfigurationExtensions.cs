// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Web.Http;

namespace Microsoft.Bot.Builder.Alexa.Integration.AspNet.WebApi
{
    public static class HttpConfigurationExtensions
    {
        public static HttpConfiguration MapAlexaBotFramework(this HttpConfiguration httpConfiguration, Action<AlexaBotConfigurationBuilder> configurer)
        {
            var options = new AlexaBotOptions();
            var optionsBuilder = new AlexaBotConfigurationBuilder(options);

            configurer(optionsBuilder);

            ConfigureAlexaBotRoutes(BuildAdapter());

            return httpConfiguration;

            AlexaAdapter BuildAdapter()
            {
                var adapter = new AlexaAdapter();

                foreach (var middleware in options.Middleware)
                {
                    adapter.Use(middleware);
                }

                return adapter;
            }

            void ConfigureAlexaBotRoutes(AlexaAdapter adapter)
            {
                var routes = httpConfiguration.Routes;
                var baseUrl = options.Paths.BasePath;

                if (!baseUrl.StartsWith("/"))
                {
                    baseUrl = baseUrl.Substring(1, baseUrl.Length - 1);
                }

                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }

                routes.MapHttpRoute(
                        "Alexa Skill Requests Handler",
                        baseUrl + options.Paths.SkillRequestsPath,
                        defaults: null,
                        constraints: null,
                        handler: new AlexaRequestHandler(adapter, options.AlexaOptions));
            }
        }
    }
}