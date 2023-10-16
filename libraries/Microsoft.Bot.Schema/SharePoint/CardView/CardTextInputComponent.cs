// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Adaptive Card Extension text input component.
    /// </summary>
    public class CardTextInputComponent : BaseCardComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardTextInputComponent"/> class.
        /// </summary>
        public CardTextInputComponent()
        {
            this.ComponentName = CardComponentName.TextInput;
        }

        /// <summary>
        /// Gets or sets the placeholder text to display in the text input.
        /// </summary>
        /// <value>Placeholder text to display.</value>
        [JsonProperty(PropertyName = "placeholder")]
        public string Placeholder { get; set; }

        /// <summary>
        /// Gets or sets the default text value of the text input.
        /// </summary>
        /// <value>Default value to display in the text input.</value>
        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the text input's button configuration.
        /// </summary>
        /// <value>Text input's button configuration.</value>
        [JsonProperty(PropertyName = "button")]
        public CardTextInputBaseButton Button { get; set; }

        /// <summary>
        /// Gets or sets properties for an optional icon, displayed in the left end of the text input.
        /// </summary>
        /// <value>Properties for an optional icon.</value>
        public CardImage IconBefore { get; set; }

        /// <summary>
        /// Gets or sets properties for an optional icon, displayed in the right end of the text input.
        /// </summary>
        /// <value>Properties for an optional icon.</value>
        public CardImage IconAfter { get; set; }
    }
}
