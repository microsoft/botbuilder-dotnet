// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.PropertyMocks
{
    /// <summary>
    /// Mock one or more property values.
    /// </summary>
    public class PropertiesMock : PropertyMock
    {
        /// <summary>
        /// Kind to serialize.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.PropertiesMock";

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertiesMock"/> class.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
        [JsonConstructor]
        public PropertiesMock([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
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
    }
}
