// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card text input.
    /// </summary>
    public partial class O365ConnectorCardTextInput : O365ConnectorCardInputBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardTextInput"/> class.
        /// </summary>
        public O365ConnectorCardTextInput()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardTextInput"/> class.
        /// </summary>
        /// <param name="type">Input type name. Possible values include:
        /// 'textInput', 'dateInput', 'multichoiceInput'.</param>
        /// <param name="id">Input Id. It must be unique per entire O365 connector card.</param>
        /// <param name="isRequired">Define if this input is a required field. Default value is false.</param>
        /// <param name="title">Input title that will be shown as the placeholder.</param>
        /// <param name="value">Default value for this input field.</param>
        /// <param name="isMultiline">Define if text input is allowed for multiple lines. Default value is false.</param>
        /// <param name="maxLength">Maximum length of text input. Default value is unlimited.</param>
        public O365ConnectorCardTextInput(string type = default, string id = default, bool? isRequired = default, string title = default, string value = default, bool? isMultiline = default, double? maxLength = default)
            : base(type, id, isRequired, title, value)
        {
            IsMultiline = isMultiline;
            MaxLength = maxLength;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets define if text input is allowed for multiple lines.
        /// Default value is false.
        /// </summary>
        /// <value>Boolean indicating if multi-line input is allowed.</value>
        [JsonProperty(PropertyName = "isMultiline")]
        public bool? IsMultiline { get; set; }

        /// <summary>
        /// Gets or sets maximum length of text input. Default value is
        /// unlimited.
        /// </summary>
        /// <value>The maximum length of text input.</value>
        [JsonProperty(PropertyName = "maxLength")]
        public double? MaxLength { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
