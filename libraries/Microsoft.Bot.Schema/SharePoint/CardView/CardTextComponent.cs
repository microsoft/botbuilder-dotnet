// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Adaptive Card Extension card text component.
    /// </summary>
    public class CardTextComponent : BaseCardComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardTextComponent"/> class.
        /// </summary>
        public CardTextComponent()
        {
            this.ComponentName = CardComponentName.Text;
        }

        /// <summary>
        /// Gets or sets the text to display.
        /// </summary>
        /// <value>Text to display.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
