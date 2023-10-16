// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Names of the supported Adaptive Card Extension Card View button styles.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
    public enum CardButtonStyle
    {
        /// <summary>
        /// Default style.
        /// </summary>
        Default,

        /// <summary>
        /// Positive (primary) style.
        /// </summary>
        Positive
    }

    /// <summary>
    /// Adaptive Card Extension card button component.
    /// </summary>
    public class CardButtonComponent : BaseCardComponent, ICardButtonBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardButtonComponent"/> class.
        /// </summary>
        public CardButtonComponent()
        {
            this.ComponentName = CardComponentName.CardButton;
        }

        /// <summary>
        /// Gets or sets the button's action.
        /// </summary>
        /// <value>Button's action.</value>
        [JsonProperty(PropertyName = "action")]
        public Action Action { get; set; }

        /// <summary>
        /// Gets or sets the text to display.
        /// </summary>
        /// <value>Text value to display in the card button.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the style of the button.
        /// </summary>
        /// <value>Style of the button.</value>
        [JsonProperty(PropertyName = "style")]
        public CardButtonStyle Style { get; set; }
    }
}
