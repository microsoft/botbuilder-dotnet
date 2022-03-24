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
        /// Name of Metadata key to be used for source filtering in <see cref="QnAMakerOptions.StrictFilters"/>.
        /// </summary>
        /// <remarks>
        /// Example: 
        /// <code> new Metadata { Name = "source_name_metadata", Value = "one_of_kb_sources.extension" } </code>
        /// can be used to get answers only from the source "one_of_kb_sources.extension".
        /// </remarks>
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
