// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Card text input button with icon.
    /// </summary>
    public class CardTextInputIconButton : CardTextInputBaseButton
    {
        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        /// <value>Icon to display in the button.</value>
        [JsonProperty(PropertyName = "icon")]
        public CardImage Icon { get; set; }
    }
}
