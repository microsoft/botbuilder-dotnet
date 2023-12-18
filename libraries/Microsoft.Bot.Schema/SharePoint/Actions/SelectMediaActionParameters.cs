// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint parameters for a select media action.
    /// </summary>
    public class SelectMediaActionParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectMediaActionParameters"/> class.
        /// </summary>
        public SelectMediaActionParameters()
        {
            // Do nothing
        }

        /// <summary>
        /// This enum contains the different types of media that can be selected.
        /// </summary>
        public enum MediaTypeOption
        {
            /// <summary>
            /// Image
            /// </summary>
            Image = 1,

            /// <summary>
            /// Audio
            /// </summary>
            Audio = 4, 

            /// <summary>
            /// Document
            /// </summary>
            Document = 8
        }

        /// <summary>
        /// Gets or Sets type of media to be selected of type <see cref="MediaTypeOption"/>.
        /// </summary>
        /// <value>This value is the type of media to be selected.</value>
        [JsonProperty(PropertyName = "mediaType")]
        public MediaTypeOption MediaType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the allow multiple capture property is enabled of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether multiple files can be selected.</value>
        [JsonProperty(PropertyName = "allowMultipleCapture")]
        public bool AllowMultipleCapture { get; set; }

        /// <summary>
        /// Gets or Sets the max size per file selected of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the max size per file selected.</value>
        [JsonProperty(PropertyName = "maxSizePerFile")]
        public int MaxSizePerFile { get; set; }

        /// <summary>
        /// Gets or Sets the supported file formats of select media action of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the supported file formats of select media action.</value>
        [JsonProperty(PropertyName = "supportedFileFormats")]
        public IEnumerable<string> SupportedFileFormats { get; set; }
    }
}
