// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint Quick View Data object.
    /// </summary>
    public class PropertyPaneChoiceGroupImageSize
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneChoiceGroupImageSize"/> class.
        /// </summary>
        public PropertyPaneChoiceGroupImageSize()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the width of the image of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the width of the choice group.</value>
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        /// <summary>
        /// Gets or Sets the height of the image of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the height of the choice group.</value>
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }
    }
}
