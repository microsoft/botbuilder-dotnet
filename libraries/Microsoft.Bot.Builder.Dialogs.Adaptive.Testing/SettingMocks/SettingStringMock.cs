// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.SettingMocks
{
    /// <summary>
    /// Mock one or more settings with string value.
    /// </summary>
    public class SettingStringMock : SettingMock
    {
        /// <summary>
        /// The kind for this class.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.SettingStringMock";

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingStringMock"/> class.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
        [JsonConstructor]
        public SettingStringMock([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets the setting assignments.
        /// </summary>
        /// <value>
        /// Setting assignments as settings.property=value pairs. Assign the settings in sequence.
        /// </value>
        [JsonProperty("assignments")]
        public List<SettingStringAssignment> Assignments { get; } = new List<SettingStringAssignment>();
    }
}
