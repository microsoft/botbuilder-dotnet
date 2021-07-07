// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Checks whether telemetry log contsain specific events.
    /// </summary>
    [DebuggerDisplay("AssertTelemetryContains")]
    public class AssertTelemetryContains : TestAction
    {
        /// <summary>
        /// Kind for the serialization.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.AssertTelemetryContains";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertTelemetryContains"/> class.
        /// </summary>
        /// <param name="path">Path to source.</param>
        /// <param name="line">Line number in source.</param>
        [JsonConstructor]
        public AssertTelemetryContains([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the description of this check.
        /// </summary>
        /// <value>Description of what this check is.</value>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets the events should be contained.
        /// </summary>
        /// <value>The events names.</value>
        [JsonProperty("events")]
        public List<string> Events { get; } = new List<string>();

        /// <inheritdoc/>
        public override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null)
        {
            var flag = true;
            IBotTelemetryClient telemetryClient = null;
            foreach (var middware in adapter.MiddlewareSet)
            {
                if (middware is TelemetryLoggerMiddleware telemetryMiddleware)
                {
                    telemetryClient = telemetryMiddleware.TelemetryClient;
                }
            }

            var msgs = new List<string>();
            foreach (var invocation in Mock.Get(telemetryClient).Invocations)
            {
                msgs.Add(invocation.Arguments[0].ToString());
            }

            foreach (var eve in Events)
            {
                if (!msgs.Contains(eve))
                {
                    flag = false;
                    break;
                }
            }

            if (flag == false)
            {
                throw new InvalidOperationException($"{Description} {string.Join(",", Events)} AssertTelemetryContains failed");
            }

            return Task.FromResult(flag);
        }
    }
}
