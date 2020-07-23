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
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.PropertiesMock";

        [JsonConstructor]
        public PropertiesMock([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
<<<<<<< HEAD
        /// Gets or sets property assignments.
=======
        /// Gets the property assignments.
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
        /// </summary>
        /// <value>
        /// Property assignments as property=value pairs. In first match first use order.
        /// </value>
        [JsonProperty("assignments")]
<<<<<<< HEAD
        public List<PropertyAssignment> Assignments { get; set; } = new List<PropertyAssignment>();
=======
        public List<PropertyAssignment> Assignments { get; } = new List<PropertyAssignment>();
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
    }
}
