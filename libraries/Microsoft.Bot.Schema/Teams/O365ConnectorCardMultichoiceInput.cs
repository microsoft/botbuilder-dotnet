// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card multiple choice input.
    /// </summary>
    public partial class O365ConnectorCardMultichoiceInput : O365ConnectorCardInputBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardMultichoiceInput"/> class.
        /// </summary>
        public O365ConnectorCardMultichoiceInput()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardMultichoiceInput"/> class.
        /// </summary>
        /// <param name="type">Input type name. Possible values include:
        /// 'textInput', 'dateInput', 'multichoiceInput'.</param>
        /// <param name="id">Input Id. It must be unique per entire O365 connector card.</param>
        /// <param name="isRequired">Define if this input is a required field. Default value is false.</param>
        /// <param name="title">Input title that will be shown as the placeholder.</param>
        /// <param name="value">Default value for this input field.</param>
        /// <param name="choices">Set of choices whose each item can be in any subtype of O365ConnectorCardMultichoiceInputChoice.</param>
        /// <param name="style">Choice item rendering style. Default value is 'compact'. Possible values include: 'compact', 'expanded'.</param>
        /// <param name="isMultiSelect">Define if this input field allows multiple selections. Default value is false.</param>
        public O365ConnectorCardMultichoiceInput(string type = default, string id = default, bool? isRequired = default, string title = default, string value = default, IList<O365ConnectorCardMultichoiceInputChoice> choices = default, string style = default, bool? isMultiSelect = default)
            : base(type, id, isRequired, title, value)
        {
            Choices = choices;
            Style = style;
            IsMultiSelect = isMultiSelect;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets set of choices whose each item can be in any subtype
        /// of O365ConnectorCardMultichoiceInputChoice.
        /// </summary>
        /// <value>The choices.</value>
        [JsonProperty(PropertyName = "choices")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<O365ConnectorCardMultichoiceInputChoice> Choices { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets choice item rendering style. Default value is
        /// 'compact'. Possible values include: 'compact', 'expanded'.
        /// </summary>
        /// <value>The style.</value>
        [JsonProperty(PropertyName = "style")]
        public string Style { get; set; }

        /// <summary>
        /// Gets or sets define if this input field allows multiple selections.
        /// Default value is false.
        /// </summary>
        /// <value>Boolean indicating if field allows multiple selections.</value>
        [JsonProperty(PropertyName = "isMultiSelect")]
        public bool? IsMultiSelect { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
