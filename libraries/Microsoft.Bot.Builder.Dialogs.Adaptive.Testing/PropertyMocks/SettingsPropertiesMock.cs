// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.PropertyMocks
{
    /// <summary>
    /// Mock one or more settings property values.
    /// </summary>
    public class SettingsPropertiesMock : PropertyMock
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.SettingsPropertiesMock";

        [JsonConstructor]
        public SettingsPropertiesMock([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets property assignments.
        /// </summary>
        /// <value>
        /// Property assignments as settings.property=value pairs. In first match first use order.
        /// </value>
        [JsonProperty("assignments")]
        public List<SettingsPropertyAssignment> Assignments { get; set; } = new List<SettingsPropertyAssignment>();
    }
}
