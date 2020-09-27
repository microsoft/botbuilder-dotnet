// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Mock one or more property values.
    /// </summary>
    [DebuggerDisplay("SetProperties")]
    public class SetProperties : TestAction
    {
        /// <summary>
        /// Kind to serialize.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.SetProperties";

        /// <summary>
        /// Initializes a new instance of the <see cref="SetProperties"/> class.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
        [JsonConstructor]
        public SetProperties([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets the property assignments.
        /// </summary>
        /// <value>
        /// Property assignments as property=value pairs. In first match first use order.
        /// </value>
        [JsonProperty("assignments")]
        public List<PropertyAssignment> Assignments { get; } = new List<PropertyAssignment>();

        /// <inheritdoc/>
        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null)
        {
            if (inspector != null)
            {
                await inspector((dc) =>
                {
                    var mockSettingsMiddleware = dc.Context.TurnState.Get<MockSettingsMiddleware>();
                    foreach (var assignment in Assignments)
                    {
                        JToken value = null;
                        var (val, valueError) = assignment.Value.TryGetValue(dc.State);
                        if (valueError != null)
                        {
                            throw new Exception($"Expression evaluation resulted in an error. Expression: {assignment.Value.ToString()}. Error: {valueError}");
                        }

                        if (val != null)
                        {
                            value = JToken.FromObject(val).DeepClone();
                        }

                        value = value?.ReplaceJTokenRecursively(dc.State);
                        var property = assignment.Property.GetValue(dc.State);
                        if (!mockSettingsMiddleware.TryAddSetting(property, value))
                        {
                            dc.State.SetValue(property, value);
                        }
                    }
                }).ConfigureAwait(false);
                Trace.TraceInformation($"[Turn Ended => SetProperties completed]");
            }
            else
            {
                throw new Exception("No inspector to use for setting properties");
            }
        }

        /// <summary>
        /// Middleware which injests mocked settings properties.
        /// </summary>
        internal class MockSettingsMiddleware : IMiddleware
        {
            private readonly string prefix = $"{ScopePath.Settings}.";
            private readonly Dictionary<string, string> mockData = new Dictionary<string, string>();

            /// <summary>
            /// Try to add key value if key is settings.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            /// <returns>If it is settings, return true.</returns>
            public bool TryAddSetting(string key, JToken value)
            {
                if (key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    if (value.Type == JTokenType.String)
                    {
                        var path = key.Substring(prefix.Length);

                        // Note that settings use : as separator in ConfigurationBuilder.
                        mockData[path.Replace('.', ':')] = value.ToString();
                    }
                    else
                    {
                        throw new NotSupportedException($"Only string is supported as value for mocking settings. {value} is not supported.");
                    }

                    return true;
                }

                return false;
            }

            /// <inheritdoc/>
            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
            {
                turnContext.TurnState.Add(this);

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
}
