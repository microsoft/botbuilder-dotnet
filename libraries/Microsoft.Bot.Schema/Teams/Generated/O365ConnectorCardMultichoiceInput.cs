// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// O365 connector card multiple choice input
    /// </summary>
    public partial class O365ConnectorCardMultichoiceInput : O365ConnectorCardInputBase
    {
        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardMultichoiceInput
        /// class.
        /// </summary>
        public O365ConnectorCardMultichoiceInput()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardMultichoiceInput
        /// class.
        /// </summary>
        /// <param name="type">Input type name. Possible values include:
        /// 'textInput', 'dateInput', 'multichoiceInput'</param>
        /// <param name="id">Input Id. It must be unique per entire O365
        /// connector card.</param>
        /// <param name="isRequired">Define if this input is a required field.
        /// Default value is false.</param>
        /// <param name="title">Input title that will be shown as the
        /// placeholder</param>
        /// <param name="value">Default value for this input field</param>
        /// <param name="choices">Set of choices whose each item can be in any
        /// subtype of O365ConnectorCardMultichoiceInputChoice.</param>
        /// <param name="style">Choice item rendering style. Default value is
        /// 'compact'. Possible values include: 'compact', 'expanded'</param>
        /// <param name="isMultiSelect">Define if this input field allows
        /// multiple selections. Default value is false.</param>
        public O365ConnectorCardMultichoiceInput(string type = default(string), string id = default(string), bool? isRequired = default(bool?), string title = default(string), string value = default(string), IList<O365ConnectorCardMultichoiceInputChoice> choices = default(IList<O365ConnectorCardMultichoiceInputChoice>), string style = default(string), bool? isMultiSelect = default(bool?))
            : base(type, id, isRequired, title, value)
        {
            Choices = choices;
            Style = style;
            IsMultiSelect = isMultiSelect;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets set of choices whose each item can be in any subtype
        /// of O365ConnectorCardMultichoiceInputChoice.
        /// </summary>
        [JsonProperty(PropertyName = "choices")]
        public IList<O365ConnectorCardMultichoiceInputChoice> Choices { get; set; }

        /// <summary>
        /// Gets or sets choice item rendering style. Default value is
        /// 'compact'. Possible values include: 'compact', 'expanded'
        /// </summary>
        [JsonProperty(PropertyName = "style")]
        public string Style { get; set; }

        /// <summary>
        /// Gets or sets define if this input field allows multiple selections.
        /// Default value is false.
        /// </summary>
        [JsonProperty(PropertyName = "isMultiSelect")]
        public bool? IsMultiSelect { get; set; }

    }
}
