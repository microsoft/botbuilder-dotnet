// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.SettingMocks
{
    /// <summary>
    /// Middleware which injests mocked settings properties.
    /// </summary>
    public class MockSettingsMiddleware : IMiddleware
    {
        private readonly Dictionary<string, string> mockData = new Dictionary<string, string>();
        private bool configured = false;
        private IConfiguration configuredConfiguration = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockSettingsMiddleware"/> class.
        /// </summary>
        /// <param name="settingMocks">Settings to mock.</param>
        public MockSettingsMiddleware(List<SettingMock> settingMocks)
        {
            foreach (var property in settingMocks)
            {
                if (property is SettingStringMock mock)
                {
                    foreach (var assignment in mock.Assignments)
                    {
                        // Note that settings use : as separator in ConfigurationBuilder.
                        var newProperty = assignment.Property.Replace('.', ':');
                        if (mockData.ContainsKey(assignment.Property))
                        {
                            mockData[newProperty] = assignment.Value;
                        }
                        else
                        {
                            mockData.Add(newProperty, assignment.Value);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (!configured)
            {
                if (mockData.Count != 0)
                {
                    IConfigurationBuilder configBuilder = new ConfigurationBuilder();

                    // We assume bot will use TurnState to store settings' configuration.
                    var previousConfig = turnContext.TurnState.Get<IConfiguration>();
                    if (previousConfig != null)
                    {
                        configBuilder = configBuilder.AddConfiguration(previousConfig);
                    }

                    configBuilder.AddInMemoryCollection(mockData);
                    configuredConfiguration = configBuilder.Build();
                    mockData.Clear();
                }

                configured = true;
            }

            if (configuredConfiguration != null)
            {
                turnContext.TurnState.Set<IConfiguration>(configuredConfiguration);
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
