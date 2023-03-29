﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Schema.SharePoint.PropertyPaneGroupField;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint Quick View Data object.
    /// </summary>
    public class PropertyPaneLinkPopupWindowProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneLinkPopupWindowProperties"/> class.
        /// </summary>
        public PropertyPaneLinkPopupWindowProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// This enum contains the different types of fields.
        /// </summary>
        public enum PopupWindowPosition
        {
            /// <summary>
            /// Center.
            /// </summary>
            Center = 0,

            /// <summary>
            /// Right Top.
            /// </summary>
            RightTop = 1,

            /// <summary>
            /// Left Top .
            /// </summary>
            LeftTop = 2,

            /// <summary>
            /// Right Bottom.
            /// </summary>
            RightBottom = 3,

            /// <summary>
            /// Left Bottom.
            /// </summary>
            LeftBottom = 4,
        }

        /// <summary>
        /// Gets or Sets the height of the pop up window of type <see cref="int"/>.
        /// </summary>
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        /// <summary>
        /// Gets or Sets the position of pop up window <see cref="PopupWindowPosition"/> enum.
        /// </summary>
        [JsonProperty(PropertyName = "positionWindowPosition")]
        public PopupWindowPosition PositionWindowPosition { get; set; }

        /// <summary>
        /// Gets or Sets the title of pop up window of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the width of the pop up window of type <see cref="int"/>.
        /// </summary>
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }
    }
}
