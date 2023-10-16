// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Card text input button with text.
    /// </summary>
    public class CardTextInputTitleButton : CardTextInputBaseButton
    {
        /// <summary>
        /// Gets or sets the text to display.
        /// </summary>
        /// <value>Text value to display in the button.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
    }
}
