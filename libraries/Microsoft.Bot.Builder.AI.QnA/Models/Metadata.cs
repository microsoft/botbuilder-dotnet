// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Represents the Metadata object sent as part of QnA Maker requests.
    /// </summary>
    [Serializable]
#pragma warning disable CA1724 // Type names should not match namespaces (we can't change this without breaking binary compat)
    public class Metadata
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        /// <summary>
        /// Key name of metadata that represents Source filters.
        /// </summary>
        public const string SourceFilterMetadataKey = "source_name_metadata";

        /// <summary>
        /// Gets or sets the name for the Metadata property.
        /// </summary>
        /// <value>A string.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value for the Metadata property.
        /// </summary>
        /// <value>A string.</value>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
