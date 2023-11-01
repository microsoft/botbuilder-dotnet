// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint parameters for a show location action.
    /// </summary>
    public class ShowLocationActionParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowLocationActionParameters"/> class.
        /// </summary>
        public ShowLocationActionParameters()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the location coordinates of type <see cref="Location"/>.
        /// </summary>
        /// <value>This value is the location to be shown.</value>
        [JsonProperty(PropertyName = "locationCoordinates")]
        public Location LocationCoordinates { get; set; }
    }
}
