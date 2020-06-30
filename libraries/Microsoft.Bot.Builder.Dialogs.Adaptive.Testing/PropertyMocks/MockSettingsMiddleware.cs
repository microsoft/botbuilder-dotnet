// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.PropertyMocks
{
    public class MockSettingsMiddleware : IMiddleware
    {
        private readonly Dictionary<string, string> mockData = new Dictionary<string, string>();

        public MockSettingsMiddleware(List<PropertyMock> properties)
        {
            foreach (var property in properties)
            {
                if (property is SettingsPropertiesMock mock)
                {
                    foreach (var assignment in mock.Assignments)
                    {
                        if (!mockData.ContainsKey(assignment.Property))
                        {
                            mockData.Add(assignment.Property.Replace('.', ':'), assignment.Value);
                        }
                    }
                }
            }
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (mockData.Count != 0)
            {
                IConfigurationBuilder configBuilder = new ConfigurationBuilder();
                var previousConfig = turnContext.TurnState.Get<IConfiguration>();
                if (previousConfig != null)
                {
                    configBuilder = configBuilder.AddConfiguration(previousConfig);
                }

                configBuilder.AddInMemoryCollection(mockData);
                turnContext.TurnState.Set<IConfiguration>(configBuilder.Build());
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
