// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// An Activity is the basic communication type for the Bot Framework 3.0 protocol.
    /// </summary>
    /// <remarks>
    /// The Activity class contains all properties that individual, more specific activities
    /// could contain. It is a superset type.
    /// </remarks>
    public partial class Activity
    {
        /// <summary>
        /// Content-type for an <see cref="Activity"/>.
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.activity";

        partial void CustomInit()
        {
        }

        /// <summary>
        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="Activity"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();
    }
}
